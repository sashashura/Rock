// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

class RockMediaPlayer {

    // Properties
    trackWatch = true;
    timerId = -1;
    watchBits: number[] = Array<number>();
    mapSize = -1;
    percentWatched = 0;
    hiddenInputIdPercentage = "";
    hiddenInputIdMap = "";
    previousPlayBit: number | null = null;
    appendToMap = true;
    resumePlaying = true;
    elementId = "";
    writeInteraction = "always";
    debug = false;
    player: Plyr;

    private previousMap = "";

    constructor( elementId: string ) {

        this.player = new Plyr(elementId);
        this.elementId = elementId;

        this.wireEvents();
    }

    get map(): string {
        return this.watchBits.join('');
    }

    set map(value: string) {
        this.previousMap = value; // store this for use if we want to append to the map        
    }

    //#region Map Methods

    // Method: Marks the current second as watched.
    markBitWatched() {
        var playBit = Math.floor(this.player.currentTime) - 1;

        // Make sure to not double count. This can occur with timings of play/pause.
        if (playBit == this.previousPlayBit) {
            return;
        }

        var currentValue = this.watchBits[playBit];

        if (currentValue < 9) {
            this.watchBits[playBit] = ++currentValue;
        }

        var unwatchedItemCount = this.watchBits.filter(item => item == 0).length;

        this.percentWatched = 1 - (unwatchedItemCount / this.mapSize);

        // Update hidden fields
        if (this.hiddenInputIdPercentage != '') {
            let input = <HTMLInputElement>document.getElementById(this.hiddenInputIdPercentage);

            if (input != null) {
                input.value = this.percentWatched.toString();
            }
            else {
                this.writeDebugMessage('Could not find input element: ' + this.hiddenInputIdPercentage);
            }
        }

        if (this.hiddenInputIdMap != '') {
            let input = <HTMLInputElement>document.getElementById(this.hiddenInputIdMap);

            if (input != null) {
                input.value = this.watchBits.join('');
            }
            else {
                this.writeDebugMessage('Could not find input element: ' + this.hiddenInputIdMap);
            }
        }

        this.writeDebugMessage(this.watchBits.join(''));

        let currentRleMap = this.toRle(this.watchBits);
        this.writeDebugMessage('RLE: ' + currentRleMap);

        this.writeDebugMessage('Player Time: ' + this.player.currentTime + ' Current Time: ' + playBit + '; Percent Watched: ' + this.percentWatched + '; Unwatched Items: ' + unwatchedItemCount + '; Map Size: ' + this.mapSize);

        this.previousPlayBit = playBit;
    }

    // Initializes maps and sets resume location if requested
    private prepareForPlay() {
        this.writeDebugMessage("Preparing the player.");

        this.initializeMap();

        this.setResume();
    }

    // Method: If resumePlaying is enabled, sets the start position at the last place the individual watched.
    private setResume() {

        this.writeDebugMessage("The video length is: " + this.player.duration);

        // Change that resume was configured
        if (this.resumePlaying == false) {
            return;
        }

        let startPosition = 0;

        for (let i = 0; i < this.watchBits.length; i++) {
            if (this.watchBits[i] == 0) {
                startPosition = i;
                break;
            }
        }

        this.player.currentTime = startPosition;
        this.writeDebugMessage("Set starting position at: " + startPosition);
        this.writeDebugMessage("The current starting position is: " + this.player.currentTime);
    }

    // Method: Sets up the map to track watching
    private initializeMap() {
        // Load an existing map if requested and it exists
        if (this.appendToMap) {
            this.loadExistingMap();
        }
        else {
            // Create blank map
            this.createBlankMap();
        }

        // Confirm map matches video
        this.validateMap();

        // Write debug message
        this.writeDebugMessage('Init Map (' + this.mapSize + '): ' + this.watchBits.join(''));
    }

    // Method: Load an existing map using the precedence order of:
    // 1. Id of provided hidden field
    // 2. Map property provided as a string (we'll convert it to an array)
    // 3. Otherwise create a blank map
    private loadExistingMap() {
        this.writeDebugMessage("Loading existing map.");

        let existingMapString = ""
        let hiddenFieldMap = this.getMapFromHiddenField();

        // If a hidden map input was provided then use that
        if (hiddenFieldMap != "") {
            existingMapString = hiddenFieldMap;
            this.writeDebugMessage("Using map from hidden field: " + existingMapString);
        }
        else if (this.previousMap != "") {
            // Map was provided as a string to the map property, we'll need to convert it to an array
            existingMapString = this.previousMap;
            this.writeDebugMessage("Map provided in .map property: " + existingMapString);
        }
        else {
            // There's no existing map to use
            this.createBlankMap();
            this.writeDebugMessage("No previous map provided, creating a blank map.");
            return;
        }

        // If existing map has a comma the format is assumed to be run length encoded. Convert
        // the RLE format to the map and return
        if (existingMapString.indexOf(',') > -1) {
            this.watchBits = this.rleToArray(existingMapString);
            this.writeDebugMessage('Map provided in RLE format.');
        }
        else {
            // Split the string into an array
            this.watchBits = existingMapString.split('').map(x=>+x);
        }

        this.mapSize = this.watchBits.length;

        // Ensure the array is the same length as the video otherwise ignore the map.
        this.validateMap();
    }

    // Checks that the size of the map array matches the length of the video. If
    // not it creates a new blank map.
    private validateMap() {
        let videoLength = Math.floor(this.player.duration) - 1;

        if (this.watchBits.length != videoLength) {
            this.writeDebugMessage('Provided map size (' + this.watchBits.length + ') did not match the video (' + videoLength + '). Using a blank map.');
            this.createBlankMap();
        }
    }

    // Method: Creates a blank map of the size of the current video
    private createBlankMap() {
        this.mapSize = Math.floor(this.player.duration) - 1;

        // Duration will be -1 if the video does not exist
        if (this.mapSize < 0) {
            this.mapSize = 0;
        }

        this.watchBits = new Array(this.mapSize).fill(0);

        this.writeDebugMessage('Blank map created of size: ' + this.mapSize);
    }
    //#endregion

    //#region Private Methods
    // ---------------------------------------

    private getMapFromHiddenField() {
        if (this.hiddenInputIdMap == "") {
            return "";
        }

        let input = <HTMLInputElement>document.getElementById(this.hiddenInputIdMap);

        return input.value;
    }

    // Takes a RLE string and returns an unencoded array
    rleToArray(value) {
        let unencoded = new Array<number>();

        let rleArray = value.split(',');

        for (var i = 0; i < rleArray.length; i++) {
            let components = rleArray[i];

            let value = parseInt(components[components.length - 1]);
            let size = parseInt(components.substring(0, components.length - 1));

            let segment = new Array(size).fill(value);

            unencoded.push(...segment);
        }

        return unencoded;
    }

    // Method: Called each second during the watch.
    private trackPlay() {
        this.markBitWatched();
    }

    // Writes debug messages
    writeDebugMessage(message: string) {
        if (this.debug) {
            console.log('RMP(' + this.elementId + '):' + message);
        }
    }

    // Method that wires up all of the play events on load
    private wireEvents() {
        // Define play event
        this.player.on('play', event => {

            // Check that player is prepped. This will happen on the 'canplay' event for HTML5
            // but will need to be checked here for YouTube and Vimeo as they do not support that
            // event.
            if (this.mapSize == -1) {
                this.prepareForPlay();
            }

            // Start play timer
            if (this.trackWatch) {
                this.timerId = setInterval(() => this.trackPlay(), 1000);
            }
        });

        // Define pause event
        this.player.on('pause', event => {

            // Clear timer
            clearInterval(this.timerId);

            // Check if we need to write a watch bit. Not checking here can lead to gaps in the map
            // depending on the timing of the play/pause. 
            this.markBitWatched();

            // TODO: Send map to interaction

            this.writeDebugMessage("Event 'pause' called.");
        });

        // Define ended event
        this.player.on('ended', event => {

            // TODO: Send map to interaction
            this.writeDebugMessage("Event 'ended' called.");
        });

        // Define loadeddata event
        this.player.on('loadeddata', event => {

            this.writeDebugMessage("Event 'loadeddata' called." + this.player.duration);
            this.prepareForPlay();
        });

        // Define ready event
        this.player.on('ready', event => {

            this.writeDebugMessage("Event 'ready' called.");
        });

        // Define canplay event. Note this is run only for HTML5 usage. Youtube and Vimeo
        // will not use this method. They will run this logic on play. This means that the 
        // the srub bar won't be in the proper resume location for YouTube and Vimeo until
        // playback starts.
        this.player.once('canplay', event => {
            this.writeDebugMessage("Event 'canplay' called.");

            this.prepareForPlay();
        });
    }

    // Takes an array and returns the RLE string for it
    // RLE mapping is that segments are separated by commas. The last character of the segment is the value
    // Example:  1100011 = 21,30,21 (two ones, three zeros, two ones).
    toRle( value: number[] | string ) : string {

        // If passed value is a string convert it to an array of numbers
        if (!Array.isArray(value)) {
            value = value.split('').map(x=>+x);
        }

        if (value.length == 0) {
            return "";
        }

        let encoding: string[] = [];
        let previous = value[0];
        let count = 1;

        for ( let i = 1; i < value.length; i++) {
            if (value[i] !== previous) {
                encoding.push( count.toString() + previous.toString() );
                count = 1;
                previous = value[i];
            } else {
                count++;
            }
        }

        // Add last pair
        encoding.push( count.toString() + previous.toString() );

        return encoding.join(',');
    }
    //#endregion
}