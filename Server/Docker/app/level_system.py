class LevelSystem:
    def __init__(self):
        pass

    def get_level(self, user) -> int:
        lvl = 0
        xp = user.level_xp
        while xp >= self.get_xp_to_reach_next_level(lvl):
            xp -= self.get_xp_to_reach_next_level(lvl)
            lvl += 1
        return lvl

    def get_xp_in_current_level(self, user) -> int:
        xp = user.level_xp
        lvl = self.get_level(user)
        xp_for_previous_levels = sum(self.get_xp_to_reach_next_level(lvl) for lvl in range(lvl))
        return xp - xp_for_previous_levels

    def get_xp_to_level_up(self, user) -> int:
        lvl = self.get_level(user)
        return self.get_xp_to_reach_next_level(lvl) - self.get_xp_in_current_level(user)

    @staticmethod
    def get_xp_to_reach_next_level(lvl: int) -> int:
        return 100 * (lvl ** 2) + 100 * lvl

    def for_user(self, user) -> tuple[int, int, int, float]:
        level = self.get_level(user)
        xp_in_current_level = self.get_xp_in_current_level(user)
        xp_to_reach_next_level = LevelSystem.get_xp_to_reach_next_level(level)
        return level, xp_in_current_level, xp_to_reach_next_level, xp_in_current_level/xp_to_reach_next_level*100


level_system = LevelSystem()
