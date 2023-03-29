import { readFile, writeFile } from "fs/promises";
import { join } from "path";
import { fileURLToPath } from "url";

type Result = {
	file_updated: boolean;
	script_path?: string;
	csproj_path?: string;
	version_path?: string;
	info?: string;
	error?: string;
};

const result: Result = {
	file_updated: false,
	// csproj_path: "?",
	// script_path: "?",
	// version_path: "?",
	// info: "?",
};

const scriptPath = fileURLToPath(import.meta.url);

const cyan = "\u001b[38;5;6m";
const spy = "\u001b[38;5;10m";

function cl(msg: string) {
	console.log(cyan + msg);
}

function toStdout(res: Result) {
	process.stdout.write(JSON.stringify(res));
}

/** Repo abs path where relPath is relative to repo root */
function toAbsPath(relPath: string) {
	return join(scriptPath, "../../../../" + relPath);
}

const numArgs = process.argv.length - 2;

if (numArgs !== 2) {
	result.error = "Expected 2 arguments, but got " + numArgs + "!";
	toStdout(result);
	//console.log("Expected 2 arguments, but got " + numArgs + "!");
	//console.log("Arg 1: Path to .csproj (relative to repo root)");
	//console.log("Arg 2: Path to version.txt file (relative to repo root)");
	process.exit(1);
}

const csprojPath = toAbsPath(process.argv[2]);
const versionTxtPath = toAbsPath(process.argv[3]);

result.script_path = scriptPath;
result.csproj_path = csprojPath;
result.version_path = versionTxtPath;

// cl("Script path: " + scriptPath);
// cl("Using .csproj file: " + csprojPath);
// cl("Using version.txt file: " + versionTxtPath);

let newVersion = "";

let version = "?";

try {
	version = await readFile(versionTxtPath, "utf-8");
} catch (error) {
	const e = "Failed to read version file.";
	result.error = typeof error === "string" ? e + " ERROR: " + error : e;
	toStdout(result);
	process.exit(1);
}

newVersion = version.trim();

//cl(`Bumping .csproj version to ${newVersion}.`);

// The regular expression to match the variable
const regex = /<Version>\d+\.\d+\.\d+(-[^+]+)?(\+.*)?<\/Version>/g;

// Read the file

let fileContent = "";

try {
	fileContent = await readFile(csprojPath, "utf-8");
} catch (error) {
	const e = "Failed to read .csproj file.";
	result.error = typeof error === "string" ? e + " ERROR: " + error : e;
	toStdout(result);
	process.exit(1);
}

// Replace the variable with the new version number using the regular expression
const modifiedData = fileContent.replace(
	regex,
	`<Version>${newVersion}</Version>`
);

if (modifiedData === fileContent) {
	result.info = "Version has not changed, skipping...";
	toStdout(result);
	process.exit(0);
}

try {
	await writeFile(csprojPath, modifiedData, "utf-8");
} catch (error) {
	const e = "Failed to write .csproj file.";
	result.error = typeof error === "string" ? e + " ERROR: " + error : e;
	toStdout(result);
	process.exit(1);
}

result.file_updated = true;
result.info = "The .csproj has been successfully modified!";
toStdout(result);
