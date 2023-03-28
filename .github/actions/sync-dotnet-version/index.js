import { readFile, writeFile } from "fs/promises";
import path from "path";
import { fileURLToPath } from "url";

const scriptPath = fileURLToPath(import.meta.url);

const cyan = "\u001b[38;5;6m";
const spy = "\u001b[38;5;10m";

function cl(msg, core) {
	core.info(spy + msg);
}

/** Repo abs path where relPath is relative to repo root */
function toAbsPath(relPath) {
	return path.join(scriptPath, "../../../../" + relPath);
}

export default async ({ github, context, core }) => {
	const { PATH: filePath } = process.env;

	let newVersion = "";

	let version = "?";

	try {
		version = await readFile(toAbsPath("version.txt"), "utf-8");
	} catch (error) {
		console.error(error);
		version = "0.1.0";
		//throw new Error("Failed to read contents of version.txt");
	}

	newVersion = version.trim();

	const csprojPath = toAbsPath(filePath);

	cl(`Bumping .csproj version to ${newVersion}.`, core);
	cl(`Filepath .csproj: ${csprojPath}`, core);

	// The regular expression to match the variable
	const regex = /<Version>\d+\.\d+\.\d+(-[^+]+)?(\+.*)?<\/Version>/g;

	// Read the file

	let fileContent = "";

	try {
		cl("Reading", core);
		fileContent = await readFile(csprojPath, "utf-8");
	} catch (error) {
		cl("Error", core);
		console.log(error);
		throw error;
	}

	//cl(fileContent, core);

	// Replace the variable with the new version number using the regular expression
	const modifiedData = fileContent.replace(
		regex,
		`<Version>${newVersion}</Version>`
	);

	if (modifiedData === fileContent) {
		cl("Version has not changed, skipping...", core);
		return false;
	}

	try {
		await writeFile(csprojPath, modifiedData, "utf-8");
	} catch (error) {
		core.error(error);
		return false;
	}

	cl(`File ${csprojPath} has been successfully modified.`, core);

	return true;
};
