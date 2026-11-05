import os
from dotenv import load_dotenv
from flask import Flask, jsonify
from Scripts.extensions import db, jwt
from Scripts.blueprints.auth import auth_bp
from Scripts.blueprints.settings import settings_bp
from Scripts.blueprints.progress import progress_bp
from Scripts.models import User, Progress, UserSettings

load_dotenv()

app = Flask(__name__)

app.config['SQLALCHEMY_DATABASE_URI'] = 'sqlite:///game.db'
app.config['SQLALCHEMY_TRACK_MODIFICATIONS'] = False
app.config['JWT_SECRET_KEY'] = os.environ['JWT_SECRET_KEY']

db.init_app(app)
jwt.init_app(app)

app.register_blueprint(auth_bp, url_prefix='/api/auth')
app.register_blueprint(settings_bp, url_prefix='/api/settings')
app.register_blueprint(progress_bp, url_prefix='/api/progress')

@app.route('/api/', methods=['GET'])
def show_description():
    return 'Сервер для игры: A wonder of two'

with app.app_context():
    db.create_all()

if __name__ == '__main__':
    app.run(debug=True)