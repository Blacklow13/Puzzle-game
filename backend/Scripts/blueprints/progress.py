from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity
from Scripts.extensions import db
from ..models import Progress

progress_bp = Blueprint("progress", __name__)


@progress_bp.route('/', methods=['GET', 'POST', 'DELETE'])
@jwt_required()
def progress():
    user_id = get_jwt_identity()

    if request.method == "POST":
        level_id = request.json.get("level_id")
        if not level_id:
            return jsonify({"error": "Нет level_id"}), 400

        existing = Progress.query.filter_by(user_id=user_id, level_id=level_id).first()
        if not existing:
            new_progress = Progress(user_id=user_id, level_id=level_id)
            db.session.add(new_progress)
            db.session.commit()

        return jsonify({"status": "success", "message": "Level saved"}), 200

    elif request.method == "DELETE":
        Progress.query.filter_by(user_id=user_id).delete()
        db.session.commit()
        return jsonify({"status": "success", "message": "Progress deleted"}), 200

    user_levels = Progress.query.filter_by(user_id=user_id).all()
    completed = [p.level_id for p in user_levels]
    return jsonify({"completed_levels": completed}), 200