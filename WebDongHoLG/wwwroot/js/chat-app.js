let conversationHistory = [];
let isOpen = false;
let isLoading = false;

// Bật/tắt cửa sổ chat
function toggleChat() {
    isOpen = !isOpen;
    document.getElementById('chat-widget').classList.toggle('open', isOpen);
    document.getElementById('icon-chat').style.display = isOpen ? 'none' : 'block';
    document.getElementById('icon-close').style.display = isOpen ? 'block' : 'none';

    if (isOpen && conversationHistory.length === 0) {
        setTimeout(() => addBotMessage("Xin chào! Tôi là Watch Advisor 🕐\nBạn đang tìm kiếm đồng hồ như thế nào? Tôi sẽ giúp bạn chọn chiếc phù hợp nhất!"), 300);
    }
}

// Vẽ tin nhắn Bot
function addBotMessage(text) {
    const msgs = document.getElementById('messages');
    const div = document.createElement('div');
    div.className = 'msg bot';
    div.innerHTML = `
      <div class="msg-avatar">⌚</div>
      <div class="msg-bubble">${text.replace(/\n/g, '<br>')}</div>
    `;
    msgs.appendChild(div);
    msgs.scrollTop = msgs.scrollHeight;
}

// Vẽ tin nhắn User
function addUserMessage(text) {
    const msgs = document.getElementById('messages');
    const div = document.createElement('div');
    div.className = 'msg user';
    div.innerHTML = `<div class="msg-bubble">${text}</div>`;
    msgs.appendChild(div);
    msgs.scrollTop = msgs.scrollHeight;
}

// Bật hiệu ứng đang gõ
function showTyping() {
    const msgs = document.getElementById('messages');
    const div = document.createElement('div');
    div.className = 'typing-indicator';
    div.id = 'typing';
    div.innerHTML = `
      <div class="msg-avatar">⌚</div>
      <div class="typing-dots"><span></span><span></span><span></span></div>
    `;
    msgs.appendChild(div);
    msgs.scrollTop = msgs.scrollHeight;
}

// Tắt hiệu ứng đang gõ
function hideTyping() {
    const t = document.getElementById('typing');
    if (t) t.remove();
}

// Hàm gửi tin nhắn chính
async function sendMessage() {
    const input = document.getElementById('chat-input');
    const text = input.value.trim();

    if (!text || isLoading) return;

    // Cập nhật giao diện
    input.value = '';
    autoResize(input);
    addUserMessage(text);
    document.getElementById('quick-replies').style.display = 'none'; // Ẩn nút gợi ý

    // Khóa trạng thái chờ
    isLoading = true;
    document.getElementById('send-btn').disabled = true;
    showTyping();

    // Lưu lịch sử
    conversationHistory.push({ role: "user", content: text });

    try {
        // GỌI SANG FILE SERVICE CHUYÊN TRÁCH Ở ĐÂY
        const reply = await ChatService.getAIResponse(conversationHistory);

        hideTyping();
        conversationHistory.push({ role: "assistant", content: reply });
        addBotMessage(reply);

    } catch (err) {
        hideTyping();
        addBotMessage("Xin lỗi, đang có sự cố kết nối. Vui lòng thử lại sau!");
    } finally {
        // Mở khóa trạng thái dù thành công hay thất bại
        isLoading = false;
        document.getElementById('send-btn').disabled = false;
        input.focus();
    }
}

// Gửi câu hỏi nhanh
function sendQuick(text) {
    document.getElementById('chat-input').value = text;
    sendMessage();
}

// Xử lý phím Enter
function handleKey(e) {
    if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        sendMessage();
    }
}

// Tự động giãn vùng gõ chữ
function autoResize(el) {
    el.style.height = 'auto';
    el.style.height = Math.min(el.scrollHeight, 100) + 'px';
}