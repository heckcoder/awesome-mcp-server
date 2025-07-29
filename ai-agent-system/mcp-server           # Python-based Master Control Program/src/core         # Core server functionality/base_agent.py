class BaseAgent:
    def __init__(self):
        self.high_level_goals = []

    def set_goals(self, goals):
        self.high_level_goals = goals

    def process_command(self, command):
        raise NotImplementedError("This method should be overridden by subclasses.")