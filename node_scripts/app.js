/*
    https://www.npmjs.com/package/torrent-search-api?activeTab=readme

    Template:
    node app.js "query" "category" "source" limit

    Example debug calls:
    node app.js "The Shawshank Redemption" "all" "all" 5
    node app.js "The Shawshank Redemption" "all" "thepiratebay" 5

    Get all provdiers and categories:
    console.log(TorrentSearchApi.getProviders());
*/

// Dangerous code - to bypass UNABLE_TO_VERIFY_LEAF_SIGNATURE
console.warn = () => {};
process.env['NODE_TLS_REJECT_UNAUTHORIZED'] = '0';

const TorrentSearchApi = require('torrent-search-api');
const providers = ['All', 'ThePirateBay', 'Yts', 'TorrentProject'];

// Enable supported providers
for (const provider of providers) {
    if (provider === 'All') {
        providers[providers.indexOf(provider)] = provider.toLowerCase();
        continue;
    }
    TorrentSearchApi.enableProvider(provider);
    providers[providers.indexOf(provider)] = provider.toLowerCase();
}


// Define search parameters
const searchQuery = process.argv[2].toLowerCase() || '';
var globalCategory = process.argv[3].toLowerCase() || 'all';
var source = process.argv[4].toLowerCase();
const resultsLimit = process.argv[5];

// Debug
// console.log('searchQuery: ' + searchQuery);
// console.log('globalCategory: ' + globalCategory);
// console.log('source: ' + source);
// console.log('resultsLimit: ' + resultsLimit);

(async () => {
    try {
        if (!searchQuery || typeof searchQuery !== 'string') return console.log('ERR-query');
        if (!globalCategory || typeof globalCategory !== 'string') return console.log('ERR-category');
        if (!providers.includes(source)) return console.log('ERR-provider');
        if (!resultsLimit) return console.log('ERR-limit');

        let globalTorrents = {};

        if (source === 'all') {
            for (const provider of providers) {
                // Skip 'All' provider
                if (provider === 'all') continue;
                // Search for provider in loop
                globalTorrents = await search(searchQuery, globalCategory, provider, resultsLimit, globalTorrents);
            }
        } else {
            // Search only in the given source/provider
            globalTorrents = await search(searchQuery, globalCategory, source, resultsLimit, globalTorrents);
            
        }

        // Final result return
        const finalResultJson = JSON.stringify(globalTorrents, '?', 2);
        console.log(finalResultJson);

    } catch (error) {
        console.log('ERR-unknown');
    }

})();

async function search(query, category, source, limit, globalTorrents) {
    const foundTorrents = await TorrentSearchApi.search([source], query, category, limit);

    // console.log(JSON.stringify(foundTorrents), '\n');

    if (source === 'thepiratebay') {
        // Check if any torrents were found - pirate bay returns 'No results returned' if no torrents are found, other providers return an empty array 
        const noResultsTorrent = foundTorrents.find(torrent => torrent.title === 'No results returned');
        if (noResultsTorrent) {
            globalTorrents[source] = [];
            return globalTorrents;
        }
    }

    let formattedTorrents = foundTorrents.map(torrent => ({
        source: torrent.provider.toLowerCase(),
        title: torrent.title,
        time: torrent.time || '?',
        size: torrent.size,
        url: getMagnetLink(source, torrent),
        seeds: getSeeds(torrent),
        peers: getPeers(torrent),
        imdb: getImdb(torrent)
    }));

    globalTorrents[source] = formattedTorrents;
    return globalTorrents;
}

// Helper functions
function getMagnetLink(providerName, torrent) {
    providerName = providerName.toLowerCase();
    if (providerName == 'thepiratebay') return torrent.magnet || '';
    else if (providerName == 'yts') return torrent.link || '';
    else if (providerName == 'torrentproject') return torrent.desc || '';
    else return '?';
}

function getSeeds(torrent) {
    try {
        return torrent.seeds == 'N/A' ? '?' : torrent.seeds.toString();
    } catch (error) {
        return '?';
    }
}

function getPeers(torrent) {
    try {
        return torrent.peers == 'N/A' ? '?' : torrent.peers.toString();
    } catch (error) {
        return '?';
    }
}

function getImdb(torrent) {
    // TODO - IMDB API call
    if (torrent.imdb) return torrent.imdb
    else return '?';
}
