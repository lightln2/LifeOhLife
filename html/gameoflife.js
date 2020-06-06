'use strict'

var lifeField;

var context;
var imageData;
var imagePixels;
var imageClampedArray;

var loadedImageData;

var performanceHtml;

var lastElapsed = performance.now();
var countedSteps = 0;
var stepsPerFrame = 1;
var manualStep = false;

var running = false;
var isDialogShown = false;

var loadedPatternType = 'random';

function Start() {
    performanceHtml = document.getElementById("performance");

    lifeField = new LifeField();

    const canvas = document.getElementById("field");
    context = canvas.getContext('2d');
    imageData = context.createImageData(LifeField.WIDTH, LifeField.HEIGHT);
    var imageBuffer = new ArrayBuffer(imageData.data.length);
    imagePixels = new Uint32Array(imageBuffer);
    imageClampedArray = new Uint8ClampedArray(imageBuffer);

    window.addEventListener('keydown', keyPressed);

    draw();
    //loadRandom();
    show('rnd-dialog');
}

function keyPressed(e) {
    if(isDialogShown) return;
    if(e.keyCode == 32) {
        e.stopPropagation();
        e.preventDefault();
        if(running) stopSimulation();
        else startSimulation();
    }
}

function step(timestamp) {
    if(running) {
        countedSteps += stepsPerFrame;
        lifeField.run(stepsPerFrame);
    }

    draw();

    if(running && !manualStep) {
        window.requestAnimationFrame(step);
    }

}

function draw() {
    lifeField.draw(imagePixels);
    imageData.data.set(imageClampedArray);
    context.putImageData(imageData, 0, 0);
    updateStatus();
}

function updateStatus() {
    const nowElapsed = performance.now();
    if (manualStep || !running || nowElapsed - lastElapsed >= 1000) {
        const stepsPerSecond = countedSteps * 1000.0 / (nowElapsed - lastElapsed);
        
        var status =
            stepsPerSecond == 0 ?
              `Generation: ${lifeField.generation}; living cells: ${lifeField.count()}` :
              `Generation: ${lifeField.generation} at ${Math.round(stepsPerSecond)} steps per second; living cells: ${lifeField.count()}`;
        performanceHtml.innerText = status;
        countedSteps = 0;
        lastElapsed = nowElapsed;
    }
}

function startSimulation() {
    if(manualStep) {
        lastElapsed = performance.now();
        countedSteps = stepsPerFrame;
        lifeField.run(stepsPerFrame);
        draw();
        setGray('control-pause');
        clearGray('control-start');
        return;
    }

    if(running) return;
    running = true;
    lastElapsed = performance.now();
    countedSteps = 0;
    clearGray('control-pause');
    setGray('control-start');
    window.requestAnimationFrame(step);
}

function stopSimulation() {
    setGray('control-pause');
    clearGray('control-start');
    running = false;
    draw();
}


function rewindSimulation() {
    stopSimulation();
    if(loadedPatternType == 'random') loadRandom();
    else if(loadedPatternType == 'image') loadImage();
    else loadLif();
    draw();
}

function selectSpeed() {
    const speedChoser = document.getElementById("speed");
    const selectedVal = speedChoser.options[speedChoser.selectedIndex].value;
    stepsPerFrame = parseInt(selectedVal);
    if(stepsPerFrame < 0) {
        stepsPerFrame = -stepsPerFrame;
        manualStep = true;
        stopSimulation();
     }
    else {
        manualStep = false;
    }
}

function loadLif() {
    stopSimulation();
    const lifContents = document.getElementById("lif").value;
    lifeField.initializeWithLifFile(lifContents);
    loadedPatternType = 'lif';
    draw();
}

function loadRandom() {
    stopSimulation();
    const seed = document.getElementById("rnd-seed").value;
    const threshold = document.getElementById("rnd-threshold").value;
    lifeField.initializeWithRandom(seed, threshold);
    loadedPatternType = 'random';
    draw();
}

function showLifDialog() {
    show('lif-dialog');
    document.getElementById('lif').focus();
    document.getElementById('lif').select();
}

function show(id) {
    if (isDialogShown) return;
    const control = document.getElementById(id);
    control.classList.remove('not-visible');
    isDialogShown = true;
}

function hide(id) {
    const control = document.getElementById(id);
    control.classList.add('not-visible');
    isDialogShown = false;
}

function setGray(id) {
    const control = document.getElementById(id);
    control.classList.add('grayed');
}

function clearGray(id) {
    const control = document.getElementById(id);
    control.classList.remove('grayed');
}

function loadImage() {
    lifeField.initializeWithImage(loadedImageData);
    stopSimulation();
    loadedPatternType = 'image';
}

function loadImg() {
    lifeField.clear();
    draw();
    const img = new Image();
    img.onload = function () {
        context.drawImage(img, LifeField.WIDTH / 2 - img.width / 2, LifeField.HEIGHT / 2 - img.height / 2);
        loadedImageData = context.getImageData(0, 0, LifeField.WIDTH, LifeField.HEIGHT);
        loadImage();
    }
    img.src = URL.createObjectURL(document.getElementById('file-img').files[0]);
    document.getElementById('file-img').value = null;
}
