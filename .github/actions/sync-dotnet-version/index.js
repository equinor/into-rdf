import { readFile } from "fs/promises";

const cyan = "\u001b[38;5;6m";
const spy = "\u001b[38;5;10m";

function cl(msg, core) {
	core.info(spy + msg);
}

export default async ({ github, context, core }) => {
	const { VERSION: newVersion, PATH: filePath } = process.env;

	let version = "?";

	try {
		version = await fs.readFile("../../../version.txt", "utf-8");
	} catch (error) {
		console.error(error);
		version = "0.0.0";
		//throw new Error("Failed to read contents of version.txt");
	}

	newVersion = version.trim();

	cl(`Bumping .csproj version to ${newVersion}.`);
	cl(`Filepath .csproj: ${filePath}`);

	// The regular expression to match the variable
	const regex = /<Version>\d+\.\d+\.\d+(-[^+]+)?(\+.*)?<\/Version>/g;

	// Read the file
	readFile("../../../" + filePath, "utf-8", (err, data) => {
		if (err) throw err;

		// Replace the variable with the new version number using the regular expression
		const modifiedData = data.replace(
			regex,
			`<Version>${newVersion}</Version>`
		);

		// Save the modified data back to the file
		writeFile(filePath, modifiedData, "utf-8", (err) => {
			if (err) throw err;

			cl(`File ${filePath} has been successfully modified.`);
		});
	});

	return true;
};
