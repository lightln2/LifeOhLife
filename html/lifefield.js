'use strict'

// wrong but useful
function random(seed) {
    var x = Math.sin(seed) * 10000;
    return x - Math.floor(x);
}


class LifeField {
    // doesn't work in Edge! see alternative syntax after the class definition
    // static WIDTH = 1920;
    // static HEIGHT = 1080;

    constructor() {
        this.field = new ArrayBuffer(LifeField.WIDTH + LifeField.WIDTH * LifeField.HEIGHT / 2);
        this.temp = new ArrayBuffer(LifeField.WIDTH + LifeField.WIDTH * LifeField.HEIGHT / 2);
        this.fieldBytes = new Uint8Array(this.field);
        this.tempBytes = new Uint8Array(this.temp);
        this.fieldInts = new Uint32Array(this.field);
        this.tempInts = new Uint32Array(this.temp);
        this.generation = 0;
    }

    get(x, y) { 
        const pos = LifeField.WIDTH / 2 + y * (LifeField.WIDTH / 2) + (x >> 1);
        if ((x & 1) == 1) return (this.fieldBytes[pos] & 0x10) == 0x10;
        else return (this.fieldBytes[pos] & 1) == 1;
    }

    set(x, y, val) {
        const pos = LifeField.WIDTH / 2 + y * (LifeField.WIDTH / 2) + (x >> 1);
        if ((x & 1) == 1) {
            if (val) this.fieldBytes[pos] |= 0x10;
            else this.fieldBytes[pos] &= (0xFF & ~0x10);
        }
        else {
            if (val) this.fieldBytes[pos] |= 0x1;
            else this.fieldBytes[pos] &= (0xFF & ~0x1);
        }
    }

    clear() {
        this.generation = 0;
        for (var x = 0; x < LifeField.WIDTH; x++) {
            for (var y = 0; y < LifeField.HEIGHT; y++) {
                this.set(x, y, false);
            }
        }
    }

    count() {
        var cnt = 0;
        for (var x = 0; x < LifeField.WIDTH; x++) {
            for (var y = 0; y < LifeField.HEIGHT; y++) {
                if (this.get(x, y)) cnt++;
            }
        }
        return cnt;
    }

    fingerprint() {
        var hash = 0;
        for (var x = 0; x < LifeField.WIDTH; x++) {
            for (var y = 0; y < LifeField.HEIGHT; y++) {
                if(this.get(x, y)) {
                    hash = hash * 31 + x;
                    hash = hash * 31 + y;
                    hash %= (1 << 31);
                }
            }
        }
        return hash;
    }
    
    initializeWithRandom(seed, threshold) {
        this.generation = 0;
        for (var x = 1; x < LifeField.WIDTH - 1; x++) {
            for (var y = 1; y < LifeField.HEIGHT - 1; y++) {
                const val = random(seed++);
                this.set(x, y, val < threshold);
            }
        }
    }

    initializeWithLifFile(contents) {
        contents = contents.trim();
        if(contents[contents.length - 1] == '!')
            this.initializeWithRle(contents);
        else 
            this.initializeWithLife15(contents);
    }

    initializeWithLife15(contents) {
        this.clear();
        const lines = contents.split("\n");
        var shiftX = 0;
        var shiftY = 0;
        var y = LifeField.HEIGHT / 2 + shiftX;
        var x = LifeField.WIDTH / 2 + shiftY;
        var index = 0;
        while(index < lines.length) {
            const line = lines[index++].trim();
            if(line.length == 0)continue;
            else if(line[0] == '#') {
                if(line[1] == 'P') {
                    x = LifeField.WIDTH / 2 + shiftX + parseInt(line.split(' ')[1]);
                    y = LifeField.HEIGHT / 2 + shiftY + parseInt(line.split(' ')[2]);
                }
            }
            else if(line[0] == '.' || line[0] == '*') {
                for(var j = 0; j < line.length; j++){
                    this.set(x + j, y, line[j] == '*');
                }
                y++;
            }
        }
    }    

    initializeWithRle(contents) {
        this.clear();
        const lines = contents.split("\n");
        var shiftX = 0;
        var shiftY = 0;
        var y = LifeField.HEIGHT / 2 + shiftX;
        var x = LifeField.WIDTH / 2 + shiftY;
        var index = 0;
        var index_x = x;
        var num = 0;
        while (index < lines.length) {
            const line = lines[index++].trim();
            if (line.length == 0) continue;
            else if (line[0] == '#') {
                if (line[1] == 'P') {
                    x = LifeField.WIDTH / 2 + shiftX + parseInt(line.split(' ')[1]);
                    y = LifeField.HEIGHT / 2 + shiftY + parseInt(line.split(' ')[2]);
                }
            }
            else if(line[0] == 'x') {
                x -= parseInt(line.split(' ')[2]) >> 1;
                y -= parseInt(line.split(' ')[5]) >> 1;
                index_x = x;
            }
            else {
                var pos = 0;
                while(pos < line.length) {
                    if(line[pos] == '$') {
                        if (num == 0) num = 1;
                        y += num;
                        index_x = x;
                        num = 0;
                    }
                    else if(line[pos] == 'b') {
                        if(num == 0) num = 1;
                        index_x += num;
                        num = 0;
                    }
                    else if(line[pos] == 'o') {
                        if (num == 0) num = 1;
                        for(var i = 0; i < num; i++){
                            this.set(index_x++, y, true);
                        }
                        num = 0;
                    }
                    else if(line[pos] >='0' && line[pos] <= '9') {
                        num = num * 10 + (line[pos]-'0');
                    }
                    pos++;
                }
            }
        
        }
    }    

    initializeWithImage(imageData) {
        for (var x = 1; x < LifeField.WIDTH - 1; x++) {
            for (var y = 1; y < LifeField.HEIGHT - 1; y++) {
                const pos = y * LifeField.WIDTH + x;
                const p = imageData.data[pos * 4] + imageData.data[pos * 4 + 1] + imageData.data[pos * 4 + 2];
                this.set(x, y, p >= 128 * 3);
            }
        }
    }

    step() {
        this.generation++;
        for(var i = 0; i < this.tempInts.length; i++) this.tempInts[i] = 0;

        for (var j = LifeField.WIDTH; j < LifeField.WIDTH * LifeField.HEIGHT / 2; j += 4)
        {
            const i = j / 4;

            const src1 = this.fieldInts[i - LifeField.WIDTH / 8];
            const src2 = this.fieldInts[i];
            const src3 = this.fieldInts[i + LifeField.WIDTH / 8];

            const src4 = this.fieldInts[i - LifeField.WIDTH / 8 - 1];
            const src5 = this.fieldInts[i - 1];
            const src6 = this.fieldInts[i + LifeField.WIDTH / 8 - 1];

            const src7 = this.fieldInts[i - LifeField.WIDTH / 8 + 1];
            const src8 = this.fieldInts[i + 1];
            const src9 = this.fieldInts[i + LifeField.WIDTH / 8 + 1];

            this.tempInts[i] += (src1 << 4) + src1 + (src1 >> 4);
            this.tempInts[i] += (src2 << 4) + (src2 >> 4);
            this.tempInts[i] += (src3 << 4) + src3 + (src3 >> 4);

            this.tempInts[i] += (src4 >> 28) + (src5 >> 28) + (src6 >> 28);
            this.tempInts[i] += (src7 << 28) + (src8 << 28) + (src9 << 28);
        }

        for (var j = LifeField.WIDTH; j < LifeField.WIDTH * LifeField.HEIGHT / 2; j += 4) {
            const i = j / 4;
            const neighours = this.tempInts[i] & 0x77777777;
            const alive = this.fieldInts[i];

            var keepAlive = ((neighours & ~0x11111111) >> 1) | (alive << 2);
            keepAlive ^= ~0x55555555;
            keepAlive &= (keepAlive >> 2);
            keepAlive &= (keepAlive >> 1);
            keepAlive &= 0x11111111;

            var makeNewLife = neighours | (alive << 3);
            makeNewLife ^= ~0x33333333;
            makeNewLife &= (makeNewLife >> 2);
            makeNewLife &= (makeNewLife >> 1);
            makeNewLife &= 0x11111111;

            this.fieldInts[i] = keepAlive | makeNewLife;
        }

        for(var y = 1; y < LifeField.WIDTH - 1; y++) {
            this.set(0, y, false);
            this.set(LifeField.WIDTH - 1, y, false);
        }
    }

    run(steps) {
        const startMillis = performance.now();
        for(var i = 0; i < steps; i++) this.step();
        const endMillis = performance.now();
        return endMillis - startMillis;
    }

    draw(imagePixels) {
        for(var i = 0; i < imagePixels.length; i += 2) {
            const val = this.fieldBytes[LifeField.WIDTH / 2 + (i>>1)];
            imagePixels[i] = (val & 1) == 0 ? 0xFF000000 : 0xFFFFFFFF;
            imagePixels[i + 1] = (val & 16) == 0 ? 0xFF000000 : 0xFFFFFFFF;
        }
    }

}

// this works in Edge; however performance is terrible in Edge anyway.
LifeField.WIDTH = 1920;
LifeField.HEIGHT = 1080;

