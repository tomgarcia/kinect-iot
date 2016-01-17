var express = require('express');
var router = express.Router();
var exec = require('child_process').exec;

var actions = {
}

var prev_command = false;

var mysql = require('mysql');
var db = mysql.createConnection({
    host: 'localhost',
    user: 'pi',
    password: 'raspberry',
    database: 'kinect_iot',
    port: '/var/run/mysqld/mysqld.sock' 
});
db.connect();

/* POST data from Kinect. */
router.post('/', function(req, res, next) {
    db.query("INSERT INTO data SET ?", {str: req.query.q}, function (err) {
        console.log(err);
    });
    exec(actions[req.query.q], {cwd: 'utilities'});
    res.send("ACK");
});

module.exports = router;
