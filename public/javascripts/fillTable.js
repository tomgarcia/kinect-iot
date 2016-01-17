$(function(){
    var table = document.getElementById('myTable');
    setInterval(function() {
        $.ajax({
            url: 'http://131.179.50.179:3000/data/',
            success: function(data) {
                console.log(data);
                var str = data.string;
                var cmd = "";
                var newRow = table.insertRow(0);
                var newCell  = newRow.insertCell(0);
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
            },
            dataType: "json"});
    },
    1000);
 });