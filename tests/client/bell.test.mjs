// The bell's labels and grouping, run on client.js itself: the functions are cut out of the
// file by their surrounding markers and evaluated with stand-in strings, without a browser.
// Usage: node tests/client/bell.test.mjs [path/to/client.js]
import fs from 'node:fs';
const src = fs.readFileSync(process.argv[2] || new URL('../../client.js', import.meta.url), 'utf8');
const grab = (start, end) => { const a = src.indexOf(start); const b = src.indexOf(end, a); if (a < 0 || b < 0) throw new Error('missing ' + start); return src.slice(a, b); };
const code = [
  grab('    const episodeSpan', '    // "Quota per Category"'),
  grab('    const upgradeKindLabel', '    const formatEpisodeTitle'),
].join('\n');
const T = { badgeNew: 'NEW', badgeUpgrade: 'UPD', badgeAdded: 'ADDED', badgeAddedTo: 'ADDED TO', kindQuality: 'Quality', kindCodec: 'Codec', kindAudio: 'Audio', kindAll: 'All' };
const mk = new Function('T', 'state', `let activeFilter = state.f; ${code}; return { processGrouping, upgradeBadgeText, unreadSeen, isUnreadCard, showsBadge, setF: f => { activeFilter = f; } };`);
const api = mk(T, { f: 'All' });
let fail = 0; const check = (n, ok) => { console.log((ok ? 'OK    ' : 'FAIL  ') + n); if (!ok) fail++; };
const now = Date.now();
// Issue #12: a title added to a monitored collection is not new to the server.
const film = { Id: 'col:aaa:111', RealItemId: '111', Type: 'Movie', Category: 'Marvel', DateCreated: new Date(now).toISOString() };
check('collection film in All says ADDED TO Marvel', api.upgradeBadgeText(film) === 'ADDED TO Marvel');
api.setF('Marvel');
check('collection film in its tab says ADDED', api.upgradeBadgeText(film) === 'ADDED');
api.setF('All');
check('library film still says NEW', api.upgradeBadgeText({ Id: '222', Type: 'Movie', Category: 'Movie' }) === 'NEW');
const ep = (id, cat) => ({ Id: id, Type: 'Episode', SeriesId: 'S1', SeriesName: 'Show', Category: cat, IndexNumber: 1, ParentIndexNumber: 1, DateCreated: new Date(now - 1000).toISOString() });
const cards = api.processGrouping([ep('e1', 'Series'), ep('e2', 'Series'), ep('col:bbb:e3', 'Anime picks')]);
check('collection episode keeps its own card', cards.length === 2 && cards.some(c => c.Category === 'Anime picks' && !c.IsGroup));
const grp = cards.find(c => c.IsGroup);
check('library group card still says NEW', grp && api.upgradeBadgeText(grp) === 'NEW');
// Opening the bell marks everything read; NEW must outlive that until the panel closes.
check('unseen card is unread', api.isUnreadCard({ Id: 'a', IsNew: true }));
check('seen card is not unread', !api.isUnreadCard({ Id: 'b', IsNew: false }));
api.unreadSeen.add('a2');
check('card marked read while open is still unread', api.isUnreadCard({ Id: 'a2', IsNew: false }));
check('group card is unread through a member', api.isUnreadCard({ Id: 'S1', IsNew: false, MemberIds: ['x', 'a2'] }));
check('unseen library card shows its badge', api.showsBadge({ Id: 'a', IsNew: true }));
api.unreadSeen.clear();
check('once seen and closed, a library card has no badge', !api.showsBadge({ Id: 'a2', IsNew: false }));
check('seen upgrade keeps its badge', api.showsBadge({ Id: 'u', IsNew: false, IsUpgrade: true }));
check('seen collection card keeps its badge', api.showsBadge({ Id: 'col:aaa:111', IsNew: false }));
console.log(fail ? fail + ' test(s) failed.' : 'All tests passed.');
process.exit(fail ? 1 : 0);
