from flask import Blueprint, request, jsonify
from werkzeug.security import generate_password_hash, check_password_hash
from flask_jwt_extended import create_access_token, jwt_required, get_jwt_identity
from Scripts.extensions import db
from ..models import User, UserSettings

auth_bp = Blueprint("auth", __name__)

@auth_bp.route('/register', methods=['POST'])
def register():
    data = request.get_json()
    login = data.get('login')
    password = data.get('password')

    if not login or not password:
        return jsonify({"error": "Заполните все поля"}), 400


    if User.query.filter_by(login=login).first():
        return jsonify({"error": "Логин занят"}), 409


    new_user = User(
        login=login,
        password_hash=generate_password_hash(password)
    )
    db.session.add(new_user)
    db.session.commit()


    new_settings = UserSettings(user_id=new_user.id)
    db.session.add(new_settings)
    db.session.commit()

    return jsonify({"status": "success", "message": "Аккаунт создан"}), 201


@auth_bp.route('/login', methods=['POST'])
def login():
    data = request.get_json()
    login = data.get('login')
    password = data.get('password')

    user = User.query.filter_by(login=login).first()


    if not user or not check_password_hash(user.password_hash, password):
        return jsonify({"error": "Неверный логин или пароль"}), 401


    access_token = create_access_token(identity=str(user.id))

    return jsonify({"token": access_token}), 200

@auth_bp.route('/change-password', methods=['PUT'])
@jwt_required()
def change_password():

    data = request.get_json(silent=True) or {}
    old_password = data.get('old_password')
    new_password = data.get('new_password')


    if not old_password or not new_password:
        return jsonify({"error": "Заполните все поля"}), 400

    if len(new_password) < 4:
        return jsonify({"error": "Новый пароль слишком короткий"}), 400

    if old_password == new_password:
        return jsonify({"error": "Новый пароль совпадает со старым"}), 400

    user_id = get_jwt_identity()
    user = User.query.filter_by(id=int(user_id)).first()

    if not user:
        return jsonify({"error": "Пользователь не найден"}), 404


    if not check_password_hash(user.password_hash, old_password):
        return jsonify({"error": "Неверный текущий пароль"}), 401


    user.password_hash = generate_password_hash(new_password)
    db.session.commit()

    return jsonify({"status": "success", "message": "Пароль изменён"}), 200