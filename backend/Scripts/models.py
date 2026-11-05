from datetime import datetime
from flask_sqlalchemy import SQLAlchemy
from Scripts.extensions import db


class User(db.Model):
    __tablename__ = 'users'

    id = db.Column(db.Integer, primary_key=True)
    login = db.Column(db.String(80), unique=True, nullable=False)
    password_hash = db.Column(db.String(128), nullable=False)
    created_at = db.Column(db.DateTime, default=datetime.utcnow)

    settings = db.relationship('UserSettings', backref='user', uselist=False, cascade="all, delete-orphan")
    progress = db.relationship('Progress', backref='user', lazy=True, cascade="all, delete-orphan")

    def __repr__(self):
        return f"<User {self.login}>"


class UserSettings(db.Model):
    __tablename__ = 'user_settings'
    user_id = db.Column(db.Integer, db.ForeignKey('users.id'), primary_key=True)
    master_volume = db.Column(db.Float, default=1.0)
    music_volume = db.Column(db.Float, default=1.0)
    sfx_volume = db.Column(db.Float, default=1.0)

    def __repr__(self):
        return f"<Settings for user_id={self.user_id}>"


class Progress(db.Model):
    __tablename__ = 'progress'

    id = db.Column(db.Integer, primary_key=True)
    user_id = db.Column(db.Integer, db.ForeignKey('users.id'), nullable=False)
    level_id = db.Column(db.Integer, nullable=False)

    def __repr__(self):
        return f"<Progress user_id={self.user_id} level={self.level_id}>"