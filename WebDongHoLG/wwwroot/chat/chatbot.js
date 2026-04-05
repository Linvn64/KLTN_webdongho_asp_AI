/* =============================================================
   LG WATCH — CHATBOT LOGIC  (v3 — white-readable, FAB anchored)
   ============================================================= */

const SUGGESTIONS = [
    "Đồng hồ nam < 5 triệu",
    "Mẫu nữ văn phòng",
    "Rolex Datejust"
];

const chatbotState = {
    isOpen: false,
    isStreaming: false,
    messages: loadFromSession()
};

/* ── Session helpers ── */
function loadFromSession() {
    try {
        const raw = sessionStorage.getItem("lgw_chat_v3");
        if (raw) return JSON.parse(raw);
    } catch (_) { }
    return [{
        role: "assistant",
        content: "Kính chào Quý khách. ✦\n\nTôi là trợ lý tư vấn của **LG WATCH** — xin được đồng hành cùng bạn trên hành trình tìm kiếm chiếc đồng hồ hoàn hảo.\n\nQuý khách đang tìm kiếm **đồng hồ nam** hay **đồng hồ nữ**?",
        time: now()
    }];
}

function saveToSession() {
    try {
        const clean = chatbotState.messages.map(m => ({ ...m, streaming: false }));
        sessionStorage.setItem("lgw_chat_v3", JSON.stringify(clean));
    } catch (_) { }
}

function now() {
    const d = new Date();
    return String(d.getHours()).padStart(2, "0") + ":" + String(d.getMinutes()).padStart(2, "0");
}

/* ══════════════════════════════════════════
   INIT
══════════════════════════════════════════ */
document.addEventListener("DOMContentLoaded", () => {
    renderChatbot();

    // Đóng khi click ra ngoài (nhưng không đóng khi click FAB)
    document.addEventListener("click", e => {
        const wrapper = document.getElementById("wc-wrapper");
        if (chatbotState.isOpen && wrapper && !wrapper.contains(e.target)) {
            closePanel();
        }
    });
});

/* ══════════════════════════════════════════
   RENDER SKELETON
══════════════════════════════════════════ */
function renderChatbot() {
    const root = document.getElementById("root");
    if (!root) return;

    root.innerHTML = `
    <div id="wc-wrapper">

      <!-- FAB — luôn hiển thị, thu nhỏ khi panel mở -->
      <button id="wc-fab" onclick="toggleChat(event)">✦</button>

      <!-- PANEL -->
      <div id="wc-panel">

        <div class="wc-header">
          <div class="wc-header-ava">✦</div>
          <div class="wc-header-info">
            <h3>LG WATCH &mdash; Tư Vấn AI</h3>
            <p>Đang trực tuyến &middot; Phản hồi ngay lập tức</p>
          </div>
          <div class="wc-header-actions">
            <button class="wc-hbtn" onclick="resetChat()" title="Xóa lịch sử">
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
                <polyline points="3 6 5 6 21 6"/>
                <path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/>
                <path d="M9 6V4h6v2"/>
              </svg>
            </button>
            <button class="wc-hbtn" onclick="closePanel()" title="Đóng">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.3" stroke-linecap="round">
                <line x1="18" y1="6" x2="6" y2="18"/>
                <line x1="6"  y1="6" x2="18" y2="18"/>
              </svg>
            </button>
          </div>
        </div>

        <div id="wc-messages" class="wc-messages"></div>

        <div class="wc-suggestions">
          ${SUGGESTIONS.map(s => `<button class="wc-chip" onclick="sendSuggested('${s}')">${s}</button>`).join("")}
        </div>

        <div class="wc-input-area">
          <textarea id="wc-input" class="wc-textarea" placeholder="Nhắn tin cho chúng tôi..." rows="1"></textarea>
          <button id="wc-send" class="wc-send" onclick="handleSend()">
            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.3" stroke-linecap="round" stroke-linejoin="round">
              <line x1="22" y1="2" x2="11" y2="13"/>
              <polygon points="22 2 15 22 11 13 2 9 22 2"/>
            </svg>
          </button>
        </div>

      </div>
    </div>`;

    const input = document.getElementById("wc-input");
    if (input) {
        input.addEventListener("keydown", e => {
            if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); handleSend(); }
        });
        input.addEventListener("input", function () {
            this.style.height = "auto";
            this.style.height = Math.min(this.scrollHeight, 90) + "px";
        });
    }

    updateMessages();
}

/* ══════════════════════════════════════════
   TOGGLE / OPEN / CLOSE
══════════════════════════════════════════ */
function toggleChat(e) {
    if (e) e.stopPropagation();
    chatbotState.isOpen ? closePanel() : openPanel();
}

function openPanel() {
    chatbotState.isOpen = true;
    const panel = document.getElementById("wc-panel");
    const fab = document.getElementById("wc-fab");
    if (panel) { panel.style.display = "flex"; panel.classList.add("open"); }
    // FAB: thu nhỏ + dính dưới, KHÔNG ẩn hẳn
    if (fab) fab.classList.add("panel-open");
    updateMessages();
}

function closePanel() {
    chatbotState.isOpen = false;
    const panel = document.getElementById("wc-panel");
    const fab = document.getElementById("wc-fab");
    if (panel) { panel.style.display = "none"; panel.classList.remove("open"); }
    if (fab) fab.classList.remove("panel-open");
}

/* ══════════════════════════════════════════
   SEND
══════════════════════════════════════════ */
function sendSuggested(text) {
    const input = document.getElementById("wc-input");
    if (input) { input.value = text; handleSend(); }
}

async function handleSend() {
    const input = document.getElementById("wc-input");
    const question = input?.value.trim();
    if (!question || chatbotState.isStreaming) return;

    input.value = "";
    input.style.height = "auto";

    chatbotState.messages.push({ role: "user", content: question, time: now() });
    updateMessages();

    chatbotState.isStreaming = true;
    setSendBtn(false);

    const aiIdx = chatbotState.messages.push({
        role: "assistant", content: "", streaming: true
    }) - 1;
    updateMessages();

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
                    chatbotState.messages[aiIdx].content = buf;
                    updateMessages();
                } catch (_) { }
            }
        }
    } catch (err) {
        chatbotState.messages[aiIdx].content = "Xin lỗi Quý khách, đã có lỗi kết nối. Vui lòng thử lại sau.";
        console.error("[LGW Chatbot]", err);
    } finally {
        chatbotState.isStreaming = false;
        chatbotState.messages[aiIdx].streaming = false;
        chatbotState.messages[aiIdx].time = now();
        setSendBtn(true);
        updateMessages();
        saveToSession();
    }
}

function setSendBtn(enabled) {
    const btn = document.getElementById("wc-send");
    if (btn) btn.disabled = !enabled;
}

/* ══════════════════════════════════════════
   RENDER MESSAGES
══════════════════════════════════════════ */
function updateMessages() {
    const container = document.getElementById("wc-messages");
    if (!container) return;

    container.innerHTML = chatbotState.messages.map((msg, i) => {
        const isUser = msg.role === "user";

        let html = (msg.content || "...")
            // Product card: [CARD:id|name|price|img]
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

        const prev = chatbotState.messages[i - 1];
        const showTime = msg.time && (!prev || prev.role !== msg.role);

        return `
            <div class="wc-msg ${isUser ? "user" : ""}">
                <div class="wc-ava">${isUser ? "👤" : "✦"}</div>
                <div class="wc-bubble-wrap">
                    <div class="wc-bubble">
                        ${html}
                        ${msg.streaming ? '<span class="wc-cursor"></span>' : ""}
                    </div>
                    ${showTime ? `<span class="wc-time">${msg.time}</span>` : ""}
                </div>
            </div>`;
    }).join("");

    container.scrollTop = container.scrollHeight;
}

/* ══════════════════════════════════════════
   RESET
══════════════════════════════════════════ */
function resetChat() {
    if (!confirm("Xóa toàn bộ lịch sử trò chuyện?")) return;
    chatbotState.messages = [{
        role: "assistant",
        content: "Lịch sử đã được làm mới. ✦\n\nTôi có thể giúp gì cho Quý khách hôm nay?",
        time: now()
    }];
    try { sessionStorage.removeItem("lgw_chat_v3"); } catch (_) { }
    updateMessages();
}