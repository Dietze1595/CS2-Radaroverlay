const fs = require("fs");
const config = JSON.parse(fs.readFileSync("config.json"));

var Bearertoken = config.FaceitToken;
var requeststate,
    express = require("express"),
    app = express(),
    http = require("http").createServer(app),
    io = require("socket.io")(http),
    bodyParser = require("body-parser"),
    axios = require("axios"),
    PlayerRoster = new Database(),
    TeamDatabase = new Database(),
    jsonParser = bodyParser.json(),
    playerID = "",
    gamename = "",
    gamestate = 0,
    NickPlayerId = "",
    playerTempElo = 0,
    ava = "",
    fl = "",
    faceitlvl = 0,
    playerWins = 0,
    playerPlayedGames = 0,
    playerCurrentWinstreak = 0,
    playerLongestWinstreak = 0,
    player20Kills = 0,
    player20HS = 0,
    player20KD = 0,
    player20KR = 0;
    roomId = 0;
app.use(express.static(__dirname + "/public")),
    app.get("/", function (e, a) {
        a.sendFile(__dirname + "/html/index.html");
    }),
    app.get("/169", function (e, a) {
        a.sendFile(__dirname + "/html/169.html");
    }),
    app.get("/43", function (e, a) {
        a.sendFile(__dirname + "/html/43.html");
    }),
    app.post("/", jsonParser, function (e, a) {
        executeRequest(e, a).catch(function (e) {});
    });
class Request {
    constructor(phase, round, activity, bomb, mysteam, playersteam, e, a, t, r, l, s, n, i, o, y, m, p, c, d, h, u, g, f, w, v, A, E, k, K, S, T, F, R) {
            (this.phase = phase),
	    (this.round = round),
            (this.activity = activity),
            (this.bomb = bomb),
            (this.mysteam = mysteam),
            (this.playersteam = playersteam),
            (this.gamestate = e),
            (this.requeststate = a),
            (this.gamename = t),
            (this.kills = r),
            (this.assists = l),
            (this.deaths = s),
            (this.mvps = n),
            (this.score = i),
            (this.r_kills = o),
            (this.name = y),
            (this.avatar = m),
            (this.flag = p),
            (this.elo = c),
            (this.faceitlevel = d),
            (this.wins = h),
            (this.playedGames = u),
            (this.currentWinstreak = g),
            (this.longestWinstreak = f),
            (this.ownTeamname = w),
            (this.enemyTeamname = v),
            (this.ownTeamAVGElo = A),
            (this.enemyTeamAVGElo = E),
            (this.winElo = k),
            (this.lossElo = K),
            (this.player20Kills = S),
            (this.player20HS = T),
            (this.player20KD = F),
            (this.player20KR = R);
    }
}
async function executeRequest(e, a) {
    var t = await processPayload(e.body, a).catch(function (e) {});
    "" !== JSON.stringify(t) && void 0 !== e.body.auth && void 0 !== e.body.auth.token && io.emit(e.body.auth.token, t), a.send("");
}
function send(e, a, t) {
    "" !== JSON.stringify(e) && void 0 !== a.auth && void 0 !== a.auth.token && io.emit(a.auth.token, e), t.send("");
}
async function processPayload(e, a) {
    if (((void 0 !== e.auth && void 0 !== e.auth.token) || returnEmpty(), void 0 === e.player.state)) {
        for (i = 0; i < 5; i++) PlayerRoster.delete("mySteamId", e.provider.steamid), TeamDatabase.delete("mySteamId", e.provider.steamid);
        returnEmpty();
    } else {
	var phase = e.map.phase;
        var round = e.player.state.health;
        var activity = e.player.activity;
        var bomb = e.round.bomb;
        var t = "";
        gamestate = 1;
        var r = e.player.match_stats.kills,
            l = e.player.match_stats.assists,
            s = e.player.match_stats.deaths,
            n = e.player.match_stats.mvps,
            o = e.player.match_stats.score,
            y = e.player.state.round_kills,
            m = e.provider.steamid,
            p = e.player.steamid;
        if (void 0 !== TeamDatabase.where("mySteamId", m)[0]) {
            if (1 === requeststate)
                if (void 0 === PlayerRoster.where("playerSteamId", p)[0]) t = new Request(0, 0, 0, 0, 1, 1, " ", 0, 0, 0, 0, 0, 0, "", "", "", 0, 0, 0, 0, 0, 0, null, null, 0, 0, 0, 0, 0, 0, 0, 0);
                else {
                    var c = TeamDatabase.where("mySteamId", m),
                        d = PlayerRoster.where("playerSteamId", p);
                    t = new Request(
			phase,
                        round,
                        activity,
                        bomb,
                        d[0].mySteamId,
                        d[0].playerSteamId,
                        gamestate,
                        requeststate,
                        c[0].gamename,
                        r,
                        l,
                        s,
                        n,
                        o,
                        y,
                        d[0].playerNickname,
                        d[0].playerAvatar,
                        d[0].playerFlag,
                        d[0].playerElo,
                        d[0].playerFaceitLevel,
                        d[0].playerWins,
                        d[0].playerPlayedGames,
                        d[0].playerCurrentWinstreak,
                        d[0].playerLongestWinstreak,
                        c[0].ownTeamname,
                        c[0].enemyTeamname,
                        c[0].ownTeamAVGElo,
                        c[0].enemyTeamAVGElo,
                        c[0].winElo,
                        c[0].lossElo,
                        d[0].player20Kills,
                        d[0].player20HS,
                        d[0].player20KD,
                        d[0].player20KR
                    );
                }
            else {
                void 0 === PlayerRoster.where("playerSteamId", p)[0] &&
                    ((gamestate = 1), send((t = new Request(0, 0, 0, 0, 1, 1, " ", 0, 0, 0, 0, 0, 0, "", "", "", 0, 0, 0, 0, 0, 0, null, null, 0, 0, 0, 0, 0, 0, 0, 0)), e, a), await getPlayerIdFromPlayer(p, m).catch(function (e) {}));
                (c = TeamDatabase.where("mySteamId", m)), (d = PlayerRoster.where("playerSteamId", p));
                t = new Request(
		    phase,
                    round,
                    activity,
                    bomb,
                    d[0].mySteamId,
                    d[0].playerSteamId,
                    gamestate,
                    requeststate,
                    c[0].gamename,
                    r,
                    l,
                    s,
                    n,
                    o,
                    y,
                    d[0].playerNickname,
                    d[0].playerAvatar,
                    d[0].playerFlag,
                    d[0].playerElo,
                    d[0].playerFaceitLevel,
                    d[0].playerWins,
                    d[0].playerPlayedGames,
                    d[0].playerCurrentWinstreak,
                    d[0].playerLongestWinstreak,
                    c[0].ownTeamname,
                    c[0].enemyTeamname,
                    c[0].ownTeamAVGElo,
                    c[0].enemyTeamAVGElo,
                    c[0].winElo,
                    c[0].lossElo,
                    d[0].player20Kills,
                    d[0].player20HS,
                    d[0].player20KD,
                    d[0].player20KR
                );
            }
            return t;
        }
        await axios
            .get("https://open.faceit.com/data/v4/players", { params: { game_player_id: m, game: "cs2" }, headers: { Authorization: "Bearer " + Bearertoken } })
            .then((e) => {
                200 !== e.status ? !0 : (playerID = e.data.player_id);
            })
            .catch(function (e) {}),
            await getFaceitMatch(playerID, m).catch(function (e) {}),
            await getLiveStats(playerID, m, roomId).catch(function (e) {}),
            returnEmpty();
    }
}




async function getLiveStats(e, a, roomLive) {
    await axios
    .get(
		"https://api.faceit.com/match/v2/match/" + roomLive,
	  )
        .then(async (response) => {
            var test = response.data.payload;
			var ownFactionNumber = checkForValue(test.teams.faction1, a) ? 1 : 2;
			var enemyFactionNumber = 1 == ownFactionNumber ? 2 : 1;

			var ownTeamname = test.teams["faction" + ownFactionNumber].name;
			var enemyTeamname = test.teams["faction" + enemyFactionNumber].name;
            var playerOwnElo = 0;
            var playerEnemyElo = 0;
            for (let x = 0; x < test.teams["faction" + ownFactionNumber].roster.length; x++){
                playerOwnElo += test.teams["faction" + ownFactionNumber].roster[x].elo;
            }
 
            for (let x = 0; x < test.teams["faction" + enemyFactionNumber].roster.length; x++) {
                playerEnemyElo += test.teams["faction" + enemyFactionNumber].roster[x].elo;
            }
 
            ownTeamAVGElo =  Math.floor(playerOwnElo / test.teams["faction" + ownFactionNumber].roster.length);		  
            enemyTeamAVGElo = Math.floor(playerEnemyElo / test.teams["faction" + enemyFactionNumber].roster.length);
	    winElo = 50;
            winElo = (test.teams["faction" + ownFactionNumber].stats == undefined) ? calculateRatingChangeOld(ownTeamAVGElo, enemyTeamAVGElo) : calculateRatingChange(test.teams["faction" + ownFactionNumber].stats.winProbability, 50);
            lossElo = 50 - winElo;
            (gamename = "Faceit"),
            TeamDatabase.insert({ gamename: gamename, mySteamId: a, ownTeamname: ownTeamname, enemyTeamname: enemyTeamname, ownTeamAVGElo: ownTeamAVGElo, enemyTeamAVGElo: enemyTeamAVGElo, winElo: winElo, lossElo: lossElo });
        })
        .catch(function (e) {});
}


async function getFaceitMatch(e, a) {
    await axios
        .get("https://api.faceit.com/match/v1/matches/groupByState", { params: { userId: e } })
        .then(async (t) => {
            if (((names = Object.getOwnPropertyNames(t.data.payload)), "VOTING" === names[0] || "READY" === names[0] || "ONGOING" === names[0])) {
                roomId = t.data.payload[names[0]][0].id;
                returnEmpty();

            } else
                PlayerRoster.delete("mySteamId", a),
                    PlayerRoster.delete("mySteamId", a),
                    PlayerRoster.delete("mySteamId", a),
                    PlayerRoster.delete("mySteamId", a),
                    TeamDatabase.delete("mySteamId", a),
                    (gamename = "MM"),
                    TeamDatabase.insert({ gamename: gamename, mySteamId: a, wnTeamname: "", enemyTeamname: "", ownTeamAVGElo: 0, enemyTeamAVGElo: 0, winElo: 0, lossElo: 0 });
        })
        .catch(function (e) {});
}
async function getEloFromPlayer(e) {
    await axios
        .get("https://open.faceit.com/data/v4/players/" + e, { headers: { Authorization: "Bearer " + Bearertoken } })
        .then((e) => {
            200 !== e.status
                ? (isNull = !0)
                : ((NickPlayerId = e.data.nickname),
                  (playerSteamId = e.data.steam_id_64),
                  (playerTempElo = e.data.games.cs2.faceit_elo),
                  (playerAvatar = e.data.avatar),
                  (playerFlag = "https://cdn-frontend.faceit.com/web/112-1536332382/src/app/assets/images-compress/flags/" + e.data.country.toUpperCase() + ".png"),
                  (playerFaceitlvl = e.data.games.cs2.skill_level));
        })
        .catch(function (e) {});
}
async function getStatsFromPlayer(e) {
    await axios
        .get("https://open.faceit.com/data/v4/players/" + e + "/stats/cs2", { headers: { Authorization: "Bearer " + Bearertoken } })
        .then((e) => {
            200 !== e.status
                ? (isNull = !0)
                : ((playerWins = e.data.lifetime.Wins), (playerPlayedGames = e.data.lifetime.Matches), (playerCurrentWinstreak = e.data.lifetime["Current Win Streak"]), (playerLongestWinstreak = e.data.lifetime["Longest Win Streak"]));
        })
        .catch(function (e) {});
}
async function getPlayerIdFromPlayer(e, a) {
    await axios
        .get("https://open.faceit.com/data/v4/players", { params: { game_player_id: e, game: "cs2" }, headers: { Authorization: "Bearer " + Bearertoken } })
        .then(async (e) => {
            200 === e.status &&
                ((requeststate = 1),
                (playerID = e.data.player_id),
                (newsteamid = e.data.games.cs2.game_player_id),
                await getStatsFromPlayer(playerID),
                await getEloFromPlayer(playerID),
                await getLast20Matches(playerID),
                PlayerRoster.insert({
                    mySteamId: a,
                    playerSteamId: newsteamid,
                    playerNickname: NickPlayerId,
                    playerElo: playerTempElo,
                    playerAvatar: playerAvatar,
                    playerFlag: playerFlag,
                    playerFaceitLevel: playerFaceitlvl,
                    playerWins: playerWins,
                    playerPlayedGames: playerPlayedGames,
                    playerCurrentWinstreak: playerCurrentWinstreak,
                    playerLongestWinstreak: playerLongestWinstreak,
                    player20Kills: playerKillsAll,
                    player20HS: playerHSAll,
                    player20KD: playerKDAll,
                    player20KR: playerKRAll,
                }),
                (requeststate = 0));
        })
        .catch(function (t) {
            404 === t.response.status &&
                ((isNull = !0),
                PlayerRoster.insert({
                    mySteamId: a,
                    playerSteamId: e,
                    playerNickname: "No Faceitaccount",
                    playerElo: 0,
                    playerAvatar: "",
                    playerFlag: "",
                    playerFaceitLevel: 0,
                    playerWins: 0,
                    playerPlayedGames: 0,
                    playerCurrentWinstreak: 0,
                    playerLongestWinstreak: 0,
                    player20Kills: 0,
                    player20HS: 0,
                    player20KD: 0,
                    player20KR: 0,
                }));
        });
}
var playerKills = 0,
    playerHS = 0,
    playerKD = 0,
    playerKR = 0,
    playerKillsAll = 0,
    playerHSAll = 0,
    playerKDAll = 0,
    playerKRAll = 0;
async function getLast20Matches(e) {
    await axios
        .get("https://api.faceit.com/stats/v1/stats/time/users/" + e + "/games/cs2?size=50", {})
        .then(async (a) => {
            if (200 === a.status) {
                for (playerKillsAll = 0, playerHSAll = 0, playerKDAll = 0, laenge = 20, playerKRAll = 0, i = 0; i < laenge; i++)
                    if (a.data[i].gameMode !== "5v5") {
                        laenge = laenge + 1;
                    } else {
                        (playerKillsAll += parseInt(a.data[i].i6)), (playerHSAll += parseInt(a.data[i].c4 * 100)), (playerKDAll += parseInt(a.data[i].c2 * 100)), (playerKRAll += parseInt(a.data[i].c3 * 100));
                    }
                (playerKillsAll = Math.round(playerKillsAll / 20)), (playerHSAll = Math.round(playerHSAll / 2000)), (playerKDAll = (playerKDAll / 2000).toFixed(2)), (playerKRAll = (playerKRAll / 2000).toFixed(2));
            }
        })
        .catch(function (e) {});
}
var matchid = "";

function calculateRatingChange(e, a) {
    var gain = Math.round(a - e * a)
    return gain;
}

function calculateRatingChangeOld(e, a) {
    var t, r;
    return (r = a - e), (t = 1 / (1 + Math.pow(10, r / 400))), Math.round(50 * (1 - t));
}

function checkForValue(e, a) {
    for (let t = 0; t < e.roster.length; t++){
		if (e.roster[t].gameId === a){
			return true;
		}
    }
    return false;
}

function Database() {
    (this.datensatz = []),
        (this.insert = function (e) {
            this.datensatz.push(e);
        }),
        (this.where = function (e, a) {
            if (!e) return this.datensatz;
            for (var t = [], r = 0; r < this.datensatz.length; r++) this.datensatz[r][e] === a && t.push(this.datensatz[r]);
            return t;
        }),
        (this.update = function (e, a, t, r) {
            for (var l = 0; l < this.datensatz.length; l++) this.datensatz[l][e] === a && (this.datensatz[l][t] = r);
        }),
        (this.delete = function (e, a) {
            if (!e) {
                var t = this.datensatz;
                return (this.datensatz = []), t;
            }
            for (var r = [], l = 0; l < this.datensatz.length; l++)
                if (this.datensatz[l][e] == a) {
                    var s = this.datensatz.splice(l, 1);
                    r = r.concat(s);
                }
            return r;
        });
}
function Sleep(e) {
    return new Promise((a) => setTimeout(a, e));
}
function returnEmpty() {
    return new Request(0, 0, 0, 0, 0, 0, " ", 0, 0, 0, 0, 0, 0, "", "", "", 0, 0, 0, 0, 0, 0, null, null, 0, 0, 0, 0, 0, 0, 0, 0);
}
http.listen(3001, function () {
    console.log("listening on *:3001");
});
