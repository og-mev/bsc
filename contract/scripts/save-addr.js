var fs = require("fs")
function save(address) {

    let addresJson = {
        address: address
    }
    const data = JSON.stringify(addresJson);
    console.log(data);
    // write JSON string to a file
    fs.writeFileSync('./deploy.json', data, (err) => {
        if (err) {
            throw err;
        }
        console.log("JSON data is saved.");
    });

}

module.exports = {save};