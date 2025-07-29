def serialize_message(message):
    import json
    return json.dumps(message)

def deserialize_message(message_str):
    import json
    return json.loads(message_str)

def create_message(message_type, payload):
    return {
        "type": message_type,
        "payload": payload
    }

def parse_message(message):
    message_type = message.get("type")
    payload = message.get("payload")
    return message_type, payload