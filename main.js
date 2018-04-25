const {app, BrowserWindow} = require('electron');
const url = require('url');
const path = require('path');
const fs = require('fs');

let win

function createWindow() {
   win = new BrowserWindow({width: 1240, height: 720})
   win.loadURL(url.format({
      pathname: path.join(__dirname, 'index.html'),
      protocol: 'file:',
      slashes: true
   }))
}

app.on('ready', createWindow)