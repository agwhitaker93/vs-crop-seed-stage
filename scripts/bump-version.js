'use strict'

const fs = require('fs')
const path = require('path')

const modinfoPath = path.resolve(__dirname, '../CropSeedStage/modinfo.json')

const modinfo = fs.readFileSync(modinfoPath).toString()

const parsedModinfo = JSON.parse(modinfo)

function bumpVersion(toBump) {
    const [major, minor, patch] = toBump.split('.')
          .map((number) => {
              return parseInt(number)
          })
    return [major, minor, patch+1].join('.')
}

parsedModinfo.version = bumpVersion(parsedModinfo.version)

fs.writeFileSync(modinfoPath, JSON.stringify(parsedModinfo, 0, 2))
