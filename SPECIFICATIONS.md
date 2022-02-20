# Specifications
Detailed below are specifications regarding the functionality of the _RULER_ client and server.

Provided in this repository are a functional client and server which abide by these specifications, though anyone could create their own alternatives.

The provided client is a C# console program which interfaces with a _RULER_ server. The provided server is a Node TypeScript application.

## The Server
The server is designed to serve program versions, as well as any accompanying data.

### File Structure
The server should serve a specific folder, which contains data that allows you to access versions. There is a central manfiest JSON file which details version locations.

Something like this:
```
served directory
├─versions-manfiest.json
├─versions
│ ├─version-x
│ │ ├─manifest.json
│ │ └─release.zip
│ ├─version-y
│ │ └─...
│ ├─version-etc.
│ │ └─...
```

This example demonstrates the general idea of a server directory layout. The root directory here is the base directory the server serves.

The `versions` folder should *always* be named `versions`. In it, folders containing all versons are hosted. These versions do not need to follow the `version-n` syntax, as they are arbitrarily named and are instead specified in `versions-manfiest.json`.

Each version folder contains two files, `manifest.json` and `release.zip`. A client should handle analysis of `manfiest.json` and unarchiving `release.zip`.

#### `versions-manfiest.json`
This manfiest file should only contain version data and specify which release is the latest.

```json
{
	"latest": "version-1.0.2",
	"versions": {
		"version-1.0.2": {
			"name": "Example"
		},
		"version-1.0.1": {
			"name": "Other Example"
		},
		"version-1.0.0": {
			"name": "First Release!"
		}
	}
}
```
There are two fields: `latest` and `versions`. `latest` should contain the key for the *latest* release, which a client would default to installing.

`versions` contains an object that currently only has the `name` field, which a client would display. The key (ie. `version-1.x.x`) just points to the associated folder (i.e. `./root/versions/version-1.x.x`).

#### `manfiest.json`
This should be included in every version folder.

```json
{
	"name": "Example",
	"desc": "I should be kept short; sweet and simple!"
}
```

Currently, not much data is stored in a `manifest.json`, just the bare minimum for displaying on a client.

#### `release.zip`
This is just an archive file containing the release. Investigation into supporting other archive formats (such as the `.tar.gz` combination, or `.rar`) may be investigated (this would add a new field to `manifest.json`).