$(document).ready(function() {
    setInterval(function() {
        $.ajax({
            url: '/data',
            success: function(data) {
                console.log(data);
            },
            dataType: "json"});
    },
    1000);
});
