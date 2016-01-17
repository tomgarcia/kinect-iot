var express = require('express');
var router = express.Router();

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
router.get('/', function(req, res, next) {
    db.query("SELECT str, time FROM data ORDER BY time DESC", function (err, results) {
        console.log(err);
        res.send(results);
    });
});

module.exports = router;
