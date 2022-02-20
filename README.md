# RULER
_RULER_ is (will be) an automated installer, updater, and launcher for [_I.RULE_](https://doctorhummer.itch.io/irule), abstracted enough to support other projects.

When launching _RULER_, if no installation is installed, _RULER_ will install the latest version. If a version is already installed, _RULER_ will check the file version and prompt an update instead. After this process has continued, _RULER_ will start an _I.RULE_ process.

## Client
The client is the core code for the console application, as well as back-end libraries reusable for other similar projects (disconnected from _I.RULE_).

**TODO: Rename Ruler.Engine to something not stupid.**

## Server
A self-hostable Node server that code serve downloads. _RULER_ is abstracted enough to allow alternative servers to be used, as long as you provide support.