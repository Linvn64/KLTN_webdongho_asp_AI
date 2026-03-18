
    document.addEventListener("DOMContentLoaded", function () {
            const form = document.getElementById('offcanvasSettings');
    const keys = ['theme', 'theme-primary', 'theme-radius'];

            keys.forEach(key => {
                const val = localStorage.getItem('tabler-' + key);
    if (val) {
                    const input = form.querySelector(`input[name="${key}"][value="${val}"]`);
    if (input) input.checked = true;
                }
            });

    form.addEventListener('change', function (e) {
                const key = e.target.name;
    const value = e.target.value;
    document.documentElement.setAttribute('data-bs-' + key, value);
    localStorage.setItem('tabler-' + key, value);
    if(key === 'theme') window.location.reload();
            });

    document.getElementById('reset-changes').addEventListener('click', function() {
        keys.forEach(k => localStorage.removeItem('tabler-' + k));
    window.location.reload();
            });
        });
