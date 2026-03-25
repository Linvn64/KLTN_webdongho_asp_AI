    function showToast(message, type) {
            const colors = {
        success: {bg: '#2ecc71', icon: 'fa-check-circle' },
    error:   {bg: '#e74c3c', icon: 'fa-times-circle' },
    warning: {bg: '#f39c12', icon: 'fa-exclamation-triangle' }
            };
    const c = colors[type] || colors.success;

    const toast = document.createElement('div');
    toast.style.cssText = `
    background: ${c.bg};
    color: white;
    padding: 14px 18px;
    border-radius: 10px;
    margin-bottom: 10px;
    display: flex;
    align-items: flex-start;
    gap: 12px;
    box-shadow: 0 4px 15px rgba(0,0,0,0.15);
    animation: slideIn 0.3s ease;
    font-size: 14px;
    line-height: 1.5;
    `;
    toast.innerHTML = `
    <i class="fa ${c.icon}" style="font-size:18px; margin-top:2px; flex-shrink:0;"></i>
    <div style="flex:1;">${message}</div>
    <button onclick="this.parentElement.remove()"
        style="background:none; border:none; color:white; font-size:18px;
                               cursor:pointer; padding:0; line-height:1; flex-shrink:0; opacity:0.8;">
        &times;
    </button>
    `;

    document.getElementById('toastContainer').appendChild(toast);

            setTimeout(() => {
        toast.style.animation = 'slideOut 0.3s ease forwards';
                setTimeout(() => toast.remove(), 300);
            }, 4000);
        }

    document.addEventListener('DOMContentLoaded', function () {
            const success = document.getElementById('_toastSuccess')?.value;
    const error   = document.getElementById('_toastError')?.value;
    const warning = document.getElementById('_toastWarning')?.value;
    if (success) showToast(success, 'success');
    if (error)   showToast(error,   'error');
    if (warning) showToast(warning, 'warning');
        });
