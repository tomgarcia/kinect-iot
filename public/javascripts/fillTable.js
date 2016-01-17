$(function(){
    var table = document.getElementById('myTable');
    setInterval(function() {
        $.ajax({
            url: '/data',
            success: function(data) {
                len = table.rows.length;
                for(i = 0; i < len; i++) {
                    table.deleteRow(0);
                }
                for(i = 0; i < data.length; i++) {
                    var str = data[i].str;
                    var cmd = "";
                    var newRow = table.insertRow();
                    var newCell  = newRow.insertCell();
                    switch(str)
                    {
                        case "LO":
                           cmd = "Lights on";
                           break;
                       case "LC":
                           cmd = "Lights off";
                           break;
                       case "LOO":
                           cmd = "Raise Brightness";
                           break;
                       case "LCC":
                           cmd = "Lower Brightness";
                           break;
                       case "OO":
                           cmd = "Raise volume";
                           break;
                       case "CC":
                           cmd = "Lower volume";
                           break;
                       case "OC":
                           cmd = "Lock door";
                           break;
                       case "CO":
                           cmd = "Open door";
                           break;
                       default:
                           cmd="Command not recognized";
                           break;
                    }
                    var newText  = document.createTextNode(cmd);
                    newCell.appendChild(newText);
                }
            },
            dataType: "json"});
    },
    1000);
 });
