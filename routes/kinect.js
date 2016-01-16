var express = require('express');
var router = express.Router();

/* POST data from Kinect. */
router.post('/', function(req, res, next) {
    console.log(req.query);
    res.send("ACK");
});

module.exports = router;
