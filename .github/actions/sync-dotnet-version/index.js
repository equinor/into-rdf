import { readFile } from "fs/promises";
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
		version = "0.0.0";
		throw new Error("Failed to read contents of version.txt");
	}

	newVersion = version.trim();

	const csprojPath = toAbsPath(filePath);

	cl(`Bumping .csproj version to ${newVersion}.`);
	cl(`Filepath .csproj: ${csprojPath}`);

	// The regular expression to match the variable
	const regex = /<Version>\d+\.\d+\.\d+(-[^+]+)?(\+.*)?<\/Version>/g;

	// Read the file
	readFile(csprojPath, "utf-8", (err, data) => {
		if (err) throw err;

		// Replace the variable with the new version number using the regular expression
		const modifiedData = data.replace(
			regex,
			`<Version>${newVersion}</Version>`
		);

		// Save the modified data back to the file
		writeFile(csprojPath, modifiedData, "utf-8", (err) => {
			if (err) throw err;

			cl(`File ${csprojPath} has been successfully modified.`);
		});
	});

	return true;
};
