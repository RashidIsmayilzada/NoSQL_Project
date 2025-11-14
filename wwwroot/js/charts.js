window.renderDonut = function (canvasId, cfg) {
    const el = document.getElementById(canvasId);
    if (!el) return;
    const ctx = el.getContext('2d');

    const data = Array.isArray(cfg.data) ? cfg.data.map(Number) : [];
    const labels = Array.isArray(cfg.labels) ? cfg.labels : [];

    const colors = ['#4e79a7', '#f28e2b', '#e15759', '#76b7b2', '#59a14f', '#edc948', '#b07aa1', '#ff9da7', '#9c755f', '#bab0ac'];

    new Chart(ctx, {
        type: 'doughnut',
        data: { labels, datasets: [{ data, backgroundColor: colors.slice(0, Math.max(data.length, 1)), borderWidth: 0 }] },
        options: { responsive: true, maintainAspectRatio: false, cutout: '65%', plugins: { legend: { position: 'bottom' }, tooltip: { enabled: true } } }
    });
};
