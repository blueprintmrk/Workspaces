/* Adapted from the Rhino book's ImageLoop.js */
function BookImages(start) {
    this.imageId = "bookimage";
    var frameUrls = [
	"gobookvs.png",
	"aqpvs.png",
	"py3bookvs.png",
	"pyqtbookvs.png"
	];
    this.urlId = "bookurl";
    this.urls = [
	"gobook.html",
	"aqpbook.html",
	"py3book.html",
	"pyqtbook.html",
	];
    this.frameInterval = 1000 * 20; // initially after 20 seconds
    this.frames = new Array(frameUrls.length);
    this.image = null;
    this.anchor = null;
    this.loaded = false;
    this.loadedFrames = 0;
    this.frameNumber = -1;
    this.timer = null;
    if (start) {
        for (var i in this.urls) {
            if (this.urls[i].indexOf(start) > -1 ) {
                this.frameNumber = i - 1;
                break;
            }
        }
    }

    var loop = this;
    function countLoadedFrames() {
        loop.loadedFrames++;
        if (loop.loadedFrames == loop.frames.length) {
            loop.loaded = true;
            loop.start();
        }
    }

    for (var i = 0; i < frameUrls.length; i++) {
        this.frames[i] = new Image();
        this.frames[i].onload = countLoadedFrames;
        this.frames[i].src = frameUrls[i];
    }

    this._displayNextFrame = function() {
        clearTimeout(loop.timer);
        loop.frameNumber = (loop.frameNumber + 1) % loop.frames.length;
        loop.image.src = loop.frames[loop.frameNumber].src;
        loop.anchor.href = loop.urls[loop.frameNumber];
        loop.frameInterval += 2000; // add 2 seconds
        loop.timer = setTimeout(loop._displayNextFrame,
                loop.frameInterval);
    };
}

BookImages.prototype.start = function() {
    if (!this.image) {
        this.image = document.getElementById(this.imageId);
    }
    if (!this.anchor) {
        this.anchor = document.getElementById(this.urlId);
    }
    this._displayNextFrame();
    this.timer = setTimeout(this._displayNextFrame, this.frameInterval);
};
