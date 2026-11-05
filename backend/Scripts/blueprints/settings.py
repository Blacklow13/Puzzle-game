from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity
from Scripts.extensions import db
from ..models import UserSettings

settings_bp = Blueprint("settings", __name__)


@settings_bp.route('/', methods=['GET', 'PUT'])
@jwt_required()
def settings():
    user_id = get_jwt_identity()

    user_settings = UserSettings.query.filter_by(user_id=user_id).first()

    if request.method == "PUT":
        data = request.json
        user_settings.master_volume = data.get('master', 1.0)
        user_settings.music_volume = data.get('music', 1.0)
        user_settings.sfx_volume = data.get('sfx', 1.0)
        db.session.commit()
        return jsonify({"status": "success", "message": "Settings Saved"}), 200

    return jsonify({
        "master": user_settings.master_volume,
        "music": user_settings.music_volume,
        "sfx": user_settings.sfx_volume
    }), 200