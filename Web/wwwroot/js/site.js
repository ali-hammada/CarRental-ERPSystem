$(document).ready(function () {

    function loadNotifications() {

        $.ajax({
            url: '/Notifications/GetNotifications',
            type: 'GET',
            success: function (res) {

                $('#notifyCount').text(res.count);

                let html = '';

                if (res.data.length === 0) {
                    html = '<li class="dropdown-item">No notifications</li>';
                }

                res.data.forEach(x => {
                    html += `
<li class="dropdown-item" onclick="markRead(${x.id})">
 ${x.title}
 <br>
 <small class="text-muted">${x.time}</small>
</li>`;
                });

                $('#notifyList').html(html);
            }
        });
    }

    window.markRead = function (id) {
        $.post('/Notifications/MarkRead?id=' + id, function () {
            loadNotifications();
        });
    }

    loadNotifications();
    setInterval(loadNotifications, 5000);
});
