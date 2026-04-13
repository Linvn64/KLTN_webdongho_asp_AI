/* =============================================================
   LG WATCH — CHAT PAGE JS
   ============================================================= */

const chatState = {
    isStreaming: false,
    messages: loadFromSession()
};

function loadFromSession() {
    try {
        const pending = restoreChatAfterLogin();
        if (pending && pending.length > 0) {
            sessionStorage.setItem("lgw_chat_v3", JSON.stringify(pending));
            return pending;
        }
        const raw = sessionStorage.getItem("lgw_chat_v3");
        if (raw) return JSON.parse(raw);
    } catch (_) { }
    return [{
        role: "assistant",
        content: "Kính chào Quý khách. ✦\n\nTôi là trợ lý tư vấn của **LG WATCH** — xin được đồng hành cùng bạn trên hành trình tìm kiếm chiếc đồng hồ hoàn hảo.\n\nQuý khách đang tìm kiếm **đồng hồ nam** hay **đồng hồ nữ**?",
        time: now()
    }];
}

function saveChatBeforeLogin() {
    try {
        const messages = chatState.messages || [];  
        localStorage.setItem("lgw_chat_pending", JSON.stringify(messages));
        localStorage.setItem("lgw_chat_pending_time", Date.now());
    } catch (_) { }
}

function restoreChatAfterLogin() {
    try {
        const savedTime = localStorage.getItem("lgw_chat_pending_time");
        if (savedTime && Date.now() - savedTime < 10 * 60 * 1000) {
            const raw = localStorage.getItem("lgw_chat_pending");
            if (raw) {
                localStorage.removeItem("lgw_chat_pending");
                localStorage.removeItem("lgw_chat_pending_time");
                return JSON.parse(raw);
            }
        }
    } catch (_) { }
    return null;
}

function saveToSession() {
    try {
        const clean = chatState.messages.map(m => ({ ...m, streaming: false }));
        sessionStorage.setItem("lgw_chat_v3", JSON.stringify(clean));
    } catch (_) { }
}

function now() {
    const d = new Date();
    return String(d.getHours()).padStart(2, "0") + ":" + String(d.getMinutes()).padStart(2, "0");
}

document.addEventListener("DOMContentLoaded", () => {
    const input = document.getElementById("chat-input");
    if (input) {
        input.addEventListener("keydown", e => {
            if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); handleSend(); }
        });
        input.addEventListener("input", function () {
            this.style.height = "auto";
            this.style.height = Math.min(this.scrollHeight, 120) + "px";
        });
    }

    if (chatState.messages.length > 0) {
        const welcome = document.getElementById("chat-welcome");
        if (welcome) welcome.style.display = "none";
        renderAllMessages();
    }
});

function sendSuggested(text) {
    const input = document.getElementById("chat-input");
    if (input) { input.value = text; handleSend(); }
}

async function handleSend() {
    const input = document.getElementById("chat-input");
    const question = input?.value.trim();
    if (!question || chatState.isStreaming) return;

    input.value = "";
    input.style.height = "auto";

    const welcome = document.getElementById("chat-welcome");
    if (welcome) welcome.style.display = "none";

    chatState.messages.push({ role: "user", content: question, time: now() });
    appendMessage(chatState.messages.length - 1);

    chatState.isStreaming = true;
    setSendBtn(false);

    const aiIdx = chatState.messages.push({ role: "assistant", content: "", streaming: true }) - 1;
    appendMessage(aiIdx);

    try {
        const res = await fetch(`/api/Chat/stream?question=${encodeURIComponent(question)}`);
        const reader = res.body.getReader();
        const dec = new TextDecoder();
        let buf = "";

        while (true) {
            const { done, value } = await reader.read();
            if (done) break;
            for (const line of dec.decode(value).split("\n")) {
                if (!line.startsWith("data: ")) continue;
                const data = line.slice(6).trim();
                if (data === "[DONE]") break;
                try {
                    buf += JSON.parse(data).text;

                    // Xử lý chưa đăng nhập
                    if (buf.includes("[LOGIN_REQUIRED]")) {
                        chatState.messages[aiIdx].content =
                            "Để nhận mã giảm giá, Quý khách vui lòng **đăng nhập** trước nhé! " +
                            "<br><button onclick='saveChatBeforeLogin(); window.location.href=\"/Account/Login?returnUrl=/Chat\"' " +
                            "style='background:#b8960c;color:#fff;border:none;padding:6px 14px;" +
                            "border-radius:6px;cursor:pointer;margin-top:8px'>" +
                            "🔐 Đăng nhập ngay</button>";
                        updateBubble(aiIdx);
                        return;
                    }

                    // Xử lý tạo voucher
                    if (buf.includes("[REQUEST_DISCOUNT:")) {
                        const match = buf.match(/\[REQUEST_DISCOUNT:(.*?)\|(\d+)\]/);
                        if (match) {
                            const maNguoiDung = match[1];
                            const phanTramGiam = parseInt(match[2]);

                            const result = await fetch("/api/Chat/request-discount", {
                                method: "POST",
                                headers: { "Content-Type": "application/json" },
                                body: JSON.stringify({ maNguoiDung, phanTramGiam })
                            });
                            const data = await result.json();

                            chatState.messages[aiIdx].content = data.success
                                ? `🎉 **Tạo mã giảm giá thành công!**<br>• Mã: **${data.maVoucher}**<br>• Giảm: **${data.phanTramGiam}%**<br>• Hết hạn: ${data.ngayHetHan}<br><br>Dùng mã này khi thanh toán nhé Quý khách!`
                                : `ℹ️ ${data.message}`;
                            updateBubble(aiIdx);
                            return;
                        }
                    }

                    chatState.messages[aiIdx].content = buf;
                    updateBubble(aiIdx);
                } catch (_) { }
            }
        }
    } catch (err) {
        chatState.messages[aiIdx].content = "Xin lỗi Quý khách, đã có lỗi kết nối. Vui lòng thử lại sau.";
        updateBubble(aiIdx);
        console.error("[LGW Chat]", err);
    } finally {
        chatState.isStreaming = false;
        chatState.messages[aiIdx].streaming = false;
        chatState.messages[aiIdx].time = now();
        setSendBtn(true);
        updateBubble(aiIdx);
        saveToSession();
    }
}

function renderBubbleHtml(msg) {
    return (msg.content || "...")
        .replace(/\[CARD:(.*?)\|(.*?)\|(.*?)\|(.*?)\]/g, (_, id, name, price, img) => `
            <div class="wc-product-card">
                <img src="${img}" class="wc-card-img" alt="${name}" onerror="this.src='/images/no-image.jpg'">
                <div class="wc-card-body">
                    <div class="wc-card-name">${name}</div>
                    <div class="wc-card-price">${price}</div>
                    <button class="wc-card-btn" onclick="location.href='/SanPhams/Detail/${id}'">XEM CHI TIẾT</button>
                </div>
            </div>`)
        .replace(/\*\*(.*?)\*\*/g, "<b>$1</b>")
        .replace(/\n/g, "<br>")
        .replace(/^- /gm, "• ");
}

function appendMessage(idx) {
    const msg = chatState.messages[idx];
    const container = document.getElementById("chat-messages");
    const isUser = msg.role === "user";

    const row = document.createElement("div");
    row.className = `msg-row ${isUser ? "user" : ""}`;
    row.id = `msg-${idx}`;

    row.innerHTML = `
        <div class="msg-ava">${isUser ? "👤" : "✦"}</div>
        <div class="msg-body">
            <div class="msg-name">${isUser ? "Bạn" : "LG WATCH AI"}</div>
            <div class="msg-bubble" id="bubble-${idx}">
                ${renderBubbleHtml(msg)}
                ${msg.streaming ? '<span class="msg-cursor"></span>' : ""}
            </div>
            ${msg.time ? `<span class="msg-time">${msg.time}</span>` : ""}
        </div>`;

    container.appendChild(row);
    container.scrollTop = container.scrollHeight;
}

function updateBubble(idx) {
    const bubble = document.getElementById(`bubble-${idx}`);
    if (!bubble) return;
    const msg = chatState.messages[idx];
    bubble.innerHTML = renderBubbleHtml(msg) + (msg.streaming ? '<span class="msg-cursor"></span>' : "");

    const row = document.getElementById(`msg-${idx}`);
    if (row && msg.time && !msg.streaming) {
        let timeEl = row.querySelector(".msg-time");
        if (!timeEl) {
            timeEl = document.createElement("span");
            timeEl.className = "msg-time";
            row.querySelector(".msg-body").appendChild(timeEl);
        }
        timeEl.textContent = msg.time;
    }

    const container = document.getElementById("chat-messages");
    container.scrollTop = container.scrollHeight;
}

function renderAllMessages() {
    chatState.messages.forEach((_, idx) => appendMessage(idx));
}

function setSendBtn(enabled) {
    const btn = document.getElementById("chat-send");
    if (btn) btn.disabled = !enabled;
}

function resetChat() {
    if (!confirm("Xóa toàn bộ lịch sử trò chuyện?")) return;
    chatState.messages = [];
    try { sessionStorage.removeItem("lgw_chat_v3"); } catch (_) { }

    const container = document.getElementById("chat-messages");
    container.innerHTML = `
        <div id="chat-welcome" class="chat-welcome">
            <div class="welcome-star">✦</div>
            <h2>Xin chào Quý khách</h2>
            <p>Tôi là trợ lý AI của LG WATCH — sẵn sàng tư vấn đồng hồ chính hãng cho bạn.</p>
            <div class="welcome-chips">
                <button class="welcome-chip" onclick="sendSuggested('Đồng hồ nam dưới 5 triệu')">Đồng hồ nam &lt; 5 triệu</button>
                <button class="welcome-chip" onclick="sendSuggested('Đồng hồ nữ văn phòng')">Đồng hồ nữ văn phòng</button>
                <button class="welcome-chip" onclick="sendSuggested('Rolex Datejust giá bao nhiêu?')">Rolex Datejust</button>
                <button class="welcome-chip" onclick="sendSuggested('Tư vấn quà tặng đồng hồ')">Quà tặng đồng hồ</button>
            </div>
        </div>`;
}

