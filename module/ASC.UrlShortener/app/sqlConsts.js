module.exports = {
    exists: "SELECT short,id FROM short_links WHERE link = ?",
    insert: "INSERT INTO short_links SET link = ?",
    update: "UPDATE short_links SET short = ? WHERE id = ?",
    find: "SELECT link FROM short_links WHERE id = ?",
};