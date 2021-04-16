"use strict";
var RockMediaPlayer = (function () {
    function RockMediaPlayer(elementId) {
        this.trackWatch = true;
        this.timerId = -1;
        this.watchBits = Array();
        this.mapSize = -1;
        this.percentWatched = 0;
        this.hiddenInputIdPercentage = "";
        this.hiddenInputIdMap = "";
        this.previousPlayBit = null;
        this.updateMap = true;
        this.resumePlaying = true;
        this.elementId = "";
        this.writeInteraction = "always";
        this.debug = false;
        this.previousMap = "";
        this.player = new Plyr(elementId);
        this.elementId = elementId;
        this.wireEvents();
    }
    Object.defineProperty(RockMediaPlayer.prototype, "map", {
        get: function () {
            return this.watchBits.join('');
        },
        set: function (value) {
            this.previousMap = value;
        },
        enumerable: false,
        configurable: true
    });
    RockMediaPlayer.prototype.markBitWatched = function () {
        var playBit = Math.floor(this.player.currentTime) - 1;
        if (playBit == this.previousPlayBit) {
            return;
        }
        var currentValue = this.watchBits[playBit];
        if (currentValue < 9) {
            this.watchBits[playBit] = ++currentValue;
        }
        var unwatchedItemCount = this.watchBits.filter(function (item) { return item == 0; }).length;
        this.percentWatched = 1 - (unwatchedItemCount / this.mapSize);
        if (this.hiddenInputIdPercentage != '') {
            var input = document.getElementById(this.hiddenInputIdPercentage);
            if (input != null) {
                input.value = this.percentWatched.toString();
            }
            else {
                this.writeDebugMessage('Could not find input element: ' + this.hiddenInputIdPercentage);
            }
        }
        if (this.hiddenInputIdMap != '') {
            var input = document.getElementById(this.hiddenInputIdMap);
            if (input != null) {
                input.value = this.watchBits.join('');
            }
            else {
                this.writeDebugMessage('Could not find input element: ' + this.hiddenInputIdMap);
            }
        }
        this.previousPlayBit = playBit;
        this.writeDebugMessage(this.watchBits.join(''));
        var currentRleMap = this.toRle(this.watchBits);
        this.writeDebugMessage('RLE: ' + currentRleMap);
        this.writeDebugMessage('Player Time: ' + this.player.currentTime + ' Current Time: ' + playBit + '; Percent Watched: ' + this.percentWatched + '; Unwatched Items: ' + unwatchedItemCount + '; Map Size: ' + this.mapSize);
    };
    RockMediaPlayer.prototype.prepareForPlay = function () {
        this.writeDebugMessage("Preparing the player.");
        this.initializeMap();
        this.setResume();
    };
    RockMediaPlayer.prototype.setResume = function () {
        this.writeDebugMessage("The video length is: " + this.player.duration);
        if (this.resumePlaying == false) {
            return;
        }
        var startPosition = 0;
        for (var i = 0; i < this.watchBits.length; i++) {
            if (this.watchBits[i] == 0) {
                startPosition = i;
                break;
            }
        }
        this.player.currentTime = startPosition;
        this.writeDebugMessage("Set starting position at: " + startPosition);
        this.writeDebugMessage("The current starting position is: " + this.player.currentTime);
    };
    RockMediaPlayer.prototype.initializeMap = function () {
        if (this.updateMap) {
            this.loadExistingMap();
        }
        else {
            this.createBlankMap();
        }
        this.validateMap();
        this.writeDebugMessage('Init Map (' + this.mapSize + '): ' + this.watchBits.join(''));
    };
    RockMediaPlayer.prototype.loadExistingMap = function () {
        this.writeDebugMessage("Loading existing map.");
        var existingMapString = "";
        var hiddenFieldMap = this.getMapFromHiddenField();
        if (hiddenFieldMap != "") {
            existingMapString = hiddenFieldMap;
            this.writeDebugMessage("Using map from hidden field: " + existingMapString);
        }
        else if (this.previousMap != "") {
            existingMapString = this.previousMap;
            this.writeDebugMessage("Map provided in .map property: " + existingMapString);
        }
        else {
            this.createBlankMap();
            this.writeDebugMessage("No previous map provided, creating a blank map.");
            return;
        }
        if (existingMapString.indexOf(',') > -1) {
            this.watchBits = this.rleToArray(existingMapString);
            this.writeDebugMessage('Map provided in RLE format.');
        }
        else {
            this.watchBits = existingMapString.split('').map(function (x) { return +x; });
        }
        this.mapSize = this.watchBits.length;
        this.validateMap();
    };
    RockMediaPlayer.prototype.validateMap = function () {
        var videoLength = Math.floor(this.player.duration) - 1;
        if (this.watchBits.length != videoLength) {
            this.writeDebugMessage('Provided map size (' + this.watchBits.length + ') did not match the video (' + videoLength + '). Using a blank map.');
            this.createBlankMap();
        }
    };
    RockMediaPlayer.prototype.createBlankMap = function () {
        this.mapSize = Math.floor(this.player.duration) - 1;
        if (this.mapSize < 0) {
            this.mapSize = 0;
        }
        this.watchBits = new Array(this.mapSize).fill(0);
        this.writeDebugMessage('Blank map created of size: ' + this.mapSize);
    };
    RockMediaPlayer.prototype.getMapFromHiddenField = function () {
        if (this.hiddenInputIdMap == "") {
            return "";
        }
        var input = document.getElementById(this.hiddenInputIdMap);
        if (input == null) {
            return "";
        }
        return input.value;
    };
    RockMediaPlayer.prototype.rleToArray = function (value) {
        var unencoded = new Array();
        var rleArray = value.split(',');
        for (var i = 0; i < rleArray.length; i++) {
            var components = rleArray[i];
            var value_1 = parseInt(components[components.length - 1]);
            var size = parseInt(components.substring(0, components.length - 1));
            var segment = new Array(size).fill(value_1);
            unencoded.push.apply(unencoded, segment);
        }
        return unencoded;
    };
    RockMediaPlayer.prototype.trackPlay = function () {
        this.markBitWatched();
    };
    RockMediaPlayer.prototype.writeDebugMessage = function (message) {
        if (this.debug) {
            console.log('RMP(' + this.elementId + '):' + message);
        }
    };
    RockMediaPlayer.prototype.wireEvents = function () {
        var _this = this;
        this.player.on('play', function (event) {
            if (_this.mapSize == -1) {
                _this.prepareForPlay();
            }
            if (_this.trackWatch) {
                _this.timerId = setInterval(function () { return _this.trackPlay(); }, 1000);
            }
        });
        this.player.on('pause', function (event) {
            clearInterval(_this.timerId);
            _this.markBitWatched();
            _this.writeDebugMessage("Event 'pause' called.");
        });
        this.player.on('ended', function (event) {
            _this.writeDebugMessage("Event 'ended' called.");
        });
        this.player.on('loadeddata', function (event) {
            _this.writeDebugMessage("Event 'loadeddata' called." + _this.player.duration);
            _this.prepareForPlay();
        });
        this.player.on('ready', function (event) {
            _this.writeDebugMessage("Event 'ready' called.");
        });
        this.player.once('canplay', function (event) {
            _this.writeDebugMessage("Event 'canplay' called.");
            _this.prepareForPlay();
        });
    };
    RockMediaPlayer.prototype.toRle = function (value) {
        if (!Array.isArray(value)) {
            value = value.split('').map(function (x) { return +x; });
        }
        if (value.length == 0) {
            return "";
        }
        var encoding = [];
        var previous = value[0];
        var count = 1;
        for (var i = 1; i < value.length; i++) {
            if (value[i] !== previous) {
                encoding.push(count.toString() + previous.toString());
                count = 1;
                previous = value[i];
            }
            else {
                count++;
            }
        }
        encoding.push(count.toString() + previous.toString());
        return encoding.join(',');
    };
    return RockMediaPlayer;
}());
//# sourceMappingURL=videoplayer.js.map