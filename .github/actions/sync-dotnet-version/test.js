import { readFile, writeFile } from "fs";

// The path of the file you want to read and modify
const filePath = "/Users/lorentz/equinor/repos/into-rdf/IntoRdf/IntoRdf.csproj";

// The new value you want to replace the variable with
const newVersion = "1.2.3-beta+build.123";

// The regular expression to match the variable
const regex = /<Version>\d+\.\d+\.\d+(-[^+]+)?(\+.*)?<\/Version>/g;

// Read the file
readFile(filePath, "utf-8", (err, data) => {
	if (err) throw err;

	// Replace the variable with the new version number using the regular expression
	const modifiedData = data.replace(regex, `<Version>${newVersion}</Version>`);

	// Save the modified data back to the file
	writeFile(filePath, modifiedData, "utf-8", (err) => {
		if (err) throw err;

		console.log(`File ${filePath} has been successfully modified.`);
	});
});
