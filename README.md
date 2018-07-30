<p align="center">
    IMG goes here...
<p>

# ECP
<p align="left">
    <!-- Version -->
    <img src="https://img.shields.io/badge/release-1.0.0-brightgreen.svg">
    <!-- <img src="https://img.shields.io/appveyor/ci/gruntjs/grunt.svg"> -->
    <!-- Docs -->
    <img src="https://img.shields.io/badge/docs-not%20found-lightgrey.svg">
    <!-- License -->
    <img src="https://img.shields.io/packagist/l/doctrine/orm.svg">
</p>

A lightweight, flexible, and extensible network communcations protocol created with security in mind and focuses on the productivity of both potential clients and servers. ECP is built on top of multiple layers of security and is meant to be a base for anyone looking to implement networking into their own applications; ECP comes bundled with AES in CBC mode for general data encryption, SHA-256/512 for checksum generation and validation, and Diffie-Hellman as the key exchange algorithm.

# Features
`Features go here...`

# TODO
`TODO goes here...`

##### New Features
- [ ] All methods now have async versions
- [ ] Thread manager needs updating
- [ ] ECPUser objects need to be generated on connection; client names should auto-generate
- [ ] Constructors and other methods need overloading
- [ ] File Transfers

##### Bugs
- [x] Shutdown commands aren't sent encrypted even if the key isn't null
- [x] Keep-alive packets aren't sent encrypted even if the key isn't null
- [ ] Closing the server before sending a {SHUTDOWN} command to the Client will cause the Client to go into an infinite loop and not shutdown
- [ ] Sending a broken handshake request such as "xxx{HANDSHAKE}" well break the tunnel forcing the user to send another handshake
- [ ] Sending a broken handshake reply such as "{HREPLY}" or "xxx{HREPLY}" or "{HREPLY}xxx" will break the tunnel requiring another handshake
- [x] Sending a shutdown command from the client causes a loop if ECPClient.Disconnect() is called afterwards
- [ ] Client doesn't process text before initial shutdown command from server
- [x] Packets from both server and client do not have any structure to them; they only have termination chars
- [x] "Broadcast All" doesn't encrypt text with client keys

# Requirements
- Windows 7 SP1 & Higher
- .NET Framwork 4.6.1

# Examples
`Examples go here...`

# Credits
`Credits go here...`

# License
Copyright © Jason Tanner (Antebyte)

All rights reserved.

The MIT License (MIT)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

Except as contained in this notice, the name of the above copyright holder
shall not be used in advertising or otherwise to promote the sale, use or
other dealings in this Software without prior written authorization.

