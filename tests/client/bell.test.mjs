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
const mk = new Function('T', 'state', `let activeFilter = state.f; let currentData = []; ${code}; return { processGrouping, upgradeBadgeText, unreadSeen, isUnreadCard, showsBadge, mergeCollectionTwins, twinIdsOf, collectionBadgeTexts, setData: d => { currentData = d; }, setF: f => { activeFilter = f; } };`);
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
// A title announced by its library and by a collection holding it: one card in "All".
const lib = { Id: 'aaaa1111-0000-0000-0000-000000000001', Type: 'Movie', Category: 'Films', IsNew: true, DateCreated: new Date(now).toISOString() };
const colTwin = { Id: 'col:bbb:aaaa1111-0000-0000-0000-000000000001', RealItemId: 'AAAA1111-0000-0000-0000-000000000001', Type: 'Movie', Category: 'Classiques', IsNew: true, DateCreated: new Date(now).toISOString() };
const colAlone = { Id: 'col:bbb:cccc3333-0000-0000-0000-000000000003', RealItemId: 'cccc3333-0000-0000-0000-000000000003', Type: 'Movie', Category: 'Classiques', IsNew: true, DateCreated: new Date(now - 1000).toISOString() };
api.setData([lib, colTwin, colAlone]);
const merged = api.mergeCollectionTwins(api.processGrouping([lib, colTwin, colAlone]));
check('twin collection card is merged into the library card', merged.length === 2 && !merged.some(c => c.Id === colTwin.Id));
check('library card carries the collection name', merged.find(c => c.Id === lib.Id).AlsoIn.join() === 'Classiques');
check('collection card without a library twin stays', merged.some(c => c.Id === colAlone.Id));
check('the bell counts the title once', merged.filter(c => c.IsNew).length === 2);
check('merged card shows ADDED TO next to NEW', api.collectionBadgeTexts(merged.find(c => c.Id === lib.Id)).join() === 'ADDED TO Classiques');
check('three collections show two names and a count', api.collectionBadgeTexts({ AlsoIn: ['A', 'B', 'C', 'D'] }).join() === 'ADDED TO A,ADDED TO B,+2');
check('dismissing the library card takes its twin too', api.twinIdsOf([lib.Id]).join() === colTwin.Id);
check('a library card without twin takes nothing more', api.twinIdsOf(['zzzz']).length === 0);
const seenLib = { ...lib, IsNew: false };
api.setData([seenLib, colTwin]);
const seenMerged = api.mergeCollectionTwins(api.processGrouping([seenLib, colTwin]));
check('a seen merged card has no NEW but keeps the collection badge', !api.showsBadge(seenMerged[0]) && api.collectionBadgeTexts(seenMerged[0]).length === 1);
api.setData([]);
check('with no collection notification nothing changes', api.mergeCollectionTwins([lib]).length === 1);
// Episodes: a library group and a collection group for the same series merge as well.
const mkEp = (id, cat, extra = {}) => ({ Id: id, Type: 'Episode', SeriesId: 'S1', SeriesName: 'Show', Category: cat, IndexNumber: 1, ParentIndexNumber: 1, IsNew: true, DateCreated: new Date(now - 500).toISOString(), ...extra });
const eps = [mkEp('e0000001-0000-0000-0000-000000000001', 'Séries'), mkEp('e0000002-0000-0000-0000-000000000002', 'Séries'),
    mkEp('col:ccc:e0000001-0000-0000-0000-000000000001', 'Classiques', { RealItemId: 'e0000001-0000-0000-0000-000000000001' }),
    mkEp('col:ccc:e0000002-0000-0000-0000-000000000002', 'Classiques', { RealItemId: 'e0000002-0000-0000-0000-000000000002' })];
api.setData(eps);
const epCards = api.mergeCollectionTwins(api.processGrouping(eps));
check('two groups of the same episodes become one card', epCards.length === 1 && epCards[0].IsGroup && epCards[0].AlsoIn.join() === 'Classiques');
// Fetch and Clear, run on client.js with a stand-in localStorage and fetch.
const fcode = [grab('    const clearAllNotifications', '    const clearCategoryNotifications'), grab('    const fetchData', '    const loadFromCache')].join('\n');
const mkFetch = (res) => {
    const store = new Map(); const calls = [];
    const ls = { getItem: k => store.has(k) ? store.get(k) : null, setItem: (k, v) => store.set(k, String(v)), removeItem: k => store.delete(k) };
    const f = new Function('S', `let isFetching = false, lastFetchTime = 0, firstLoadDone = S.firstLoadDone, currentData = S.currentData, groupedData = [], allCards = [], previousDataIds = new Set(), pollTimeout = null, retryDelay = 2000, lastPulseTime = 0;
        const localStorage = S.ls, document = { getElementById: () => null, querySelector: () => null }, setTimeout = (fn, ms) => { S.calls.push('retry'); return 1; }, clearTimeout = () => {};
        const nsKey = k => 'ns-' + k, getUserId = () => 'u1', serverUrl = p => p, getAuthHeaders = () => ({}), updateBadge = () => {}, updateList = () => {}, markVisibleAsRead = () => {}, recalculateNewStatus = () => {};
        const fetch = async () => S.res; ${fcode}; return { resetThrottle: () => { lastFetchTime = 0; }, fetchData, clearAllNotifications, get firstLoadDone() { return firstLoadDone; } };`);
    const S = { ls, calls, res, firstLoadDone: false, currentData: [] };
    return { store, calls, S, api: f(S) };
};
{
    const r = mkFetch({ status: 304, ok: false, headers: { get: () => null } });
    r.store.set('ns-etag', '"x"');
    await r.api.fetchData();
    check('a 304 on a never-loaded bell asks again', r.calls.includes('retry') && !r.store.has('ns-etag'));
    // An empty list that did load: its 304 is the answer, not a reason to refetch.
    const r2 = mkFetch({ status: 200, ok: true, json: async () => [], headers: { get: () => '"e1"' } });
    await r2.api.fetchData();
    r2.S.res = { status: 304, ok: false, headers: { get: () => null } }; r2.api.resetThrottle(); r2.calls.length = 0;
    await r2.api.fetchData();
    check('a 304 after an empty list was loaded does not loop', !r2.calls.includes('retry') && r2.store.has('ns-etag'));
    const r3 = mkFetch({ ok: true, status: 200 });
    r3.store.set('ns-data', '[{"Id":"a"}]'); r3.store.set('ns-data-ts', '1'); r3.store.set('ns-etag', '"x"');
    await r3.api.clearAllNotifications();
    check('Clear also empties the stored list', !r3.store.has('ns-data') && !r3.store.has('ns-data-ts') && !r3.store.has('ns-etag'));
}
// TV: the remote's focus after the last card goes, and syntax the Chromium 56 of older TVs rejects.
const restore = new Function(grab('    const tvRestoreIndex', '    // A redraw replaces the elements') + '; return tvRestoreIndex;')();
const row = (cls) => [{ classList: { contains: c => c === cls } }];
const rowsNow = [row('filter-pill'), row('dropdown-item'), row('dropdown-item'), row('footer-tools')];
check('after the last card goes, focus stops on a card, not on "Clear list"', restore(rowsNow, 4, 1) === 2);
check('a focus left on the footer stays there', restore(rowsNow, 3, 1) === 3);
check('focus on a middle card stays', restore(rowsNow, 2, 1) === 2);
check('first open lands on the first card', restore(rowsNow, null, 1) === 1);
const codeOnly = src.split('\n').filter(l => !/^\s*\/\//.test(l)).join('\n')
    .replace(/'(?:[^'\\\n]|\\.)*'|"(?:[^"\\\n]|\\.)*"|`(?:[^`\\]|\\.)*`/g, '""').replace(/\/\/[^\n]*/g, '');
check('no ?? (Chromium 56)', !/\?\?/.test(codeOnly));
check('no ?. (Chromium 56)', !/\?\.[A-Za-z_$[(]/.test(codeOnly));
check('no object spread (Chromium 56)', !/\{\s*\.\.\.[A-Za-z_$]/.test(codeOnly));
api.setData([]);
console.log(fail ? fail + ' test(s) failed.' : 'All tests passed.');
process.exit(fail ? 1 : 0);
