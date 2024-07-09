from datetime import datetime

last_update: datetime = datetime(1970, 1, 1)
refresh_every_x_seconds = 60 * 5 # fives minutes

_leaderboard: dict[str, list[tuple]] = {}


def _calculate_leaderboard():
    from models.UserProgress import UserProgress
    all_progress: list[UserProgress] = UserProgress.get_all()
    users = {}
    users_stats = {}
    for progress in all_progress:
        u_id = progress.user_id
        if u_id not in users:
            u = progress.get_user()
            if u is not None:
                users[u_id] = u.username
            else:
                users[u_id] = 'Unknown User'
        u_name = users[u_id]
        if u_name not in users_stats:
            users_stats[u_name] = [0, 0, 0, 0, 0]
        stats = users_stats[u_name]
        stats[0] += progress.kills
        stats[1] += progress.deaths
        stats[2] += progress.total_games
        stats[3] += progress.total_completions
        stats[4] += sum([progress.star_1, progress.star_2, progress.star_3])

    sort = lambda index: sorted(users_stats.items(), key=lambda item: item[1][index], reverse=True)

    local_lb = []

    for i in range(0, 5):
        local_lb.append([(user_name, str(stats[i])) for user_name, stats in sort(i)])

    final_lb = {}
    for th, tb in zip(['kills', 'deaths', 'games', 'completed', 'stars'], local_lb):
        final_lb[th] = tb

    global _leaderboard
    _leaderboard = final_lb


def get_leaderboard() -> dict[str, list[tuple]]:
    global last_update
    if len(_leaderboard) == 0 or (datetime.now() - last_update).seconds >= refresh_every_x_seconds:
        last_update = datetime.now()
        _calculate_leaderboard()
    return _leaderboard


def find_player(board: dict[str, list[tuple]], user_name: str) -> dict[str, tuple]:
    player_stats = {}
    for stat, lb in board.items():
        player_ranking = next(((index+1, t[0], t[1]) for index, t in enumerate(lb) if t[0] == user_name), None)
        player_stats[stat] = player_ranking
    return player_stats
