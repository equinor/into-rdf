import { readFile, writeFile } from "fs/promises";
import { join } from "path";
import { fileURLToPath } from "url";

const scriptPath = fileURLToPath(import.meta.url);

const cyan = "\u001b[38;5;6m";
const spy = "\u001b[38;5;10m";

function cl(msg: string) {
	console.log(cyan + msg);
}

/** Repo abs path where relPath is relative to repo root */
function toAbsPath(relPath: string) {
	return join(scriptPath, "../../../../" + relPath);
}

const numArgs = process.argv.length - 2;

if (numArgs !== 2) {
	console.log("Expected 2 arguments, but got " + numArgs + "!");
	console.log("Arg 1: Path to .csproj (relative to repo root)");
	console.log("Arg 2: Path to version.txt file (relative to repo root)");
	process.exit(1);
}

const csprojPath = toAbsPath(process.argv[2]);
const versionTxtPath = toAbsPath(process.argv[3]);

cl("Script path: " + scriptPath);
cl("Using .csproj file: " + csprojPath);
cl("Using version.txt file: " + versionTxtPath);

let newVersion = "";

let version = "?";

try {
	version = await readFile(versionTxtPath, "utf-8");
} catch (error) {
	console.error(error);
	process.exit(1);
}

newVersion = version.trim();

cl(`Bumping .csproj version to ${newVersion}.`);

// The regular expression to match the variable
const regex = /<Version>\d+\.\d+\.\d+(-[^+]+)?(\+.*)?<\/Version>/g;

// Read the file

let fileContent = "";

try {
	fileContent = await readFile(csprojPath, "utf-8");
} catch (error) {
	console.log("Failed to read .csproj file.");
	console.log(error);
	process.exit(1);
}

// Replace the variable with the new version number using the regular expression
const modifiedData = fileContent.replace(
	regex,
	`<Version>${newVersion}</Version>`
);

if (modifiedData === fileContent) {
	console.log("Version has not changed, skipping...");
	process.exit(1);
}

try {
	await writeFile(csprojPath, modifiedData, "utf-8");
} catch (error) {
	console.log("Failed to write .csproj file.");
	console.log(error);
	process.exit(1);
}

cl(`The .csproj has been successfully modified!`);
