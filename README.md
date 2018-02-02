# Exchange Polling Job

Stateful exchange polling job supplements Exchange Connector.

Service polls the list of exchanges to:
1. Get list of positions and accounts from exchanges that does not support websockets.
2. Control non-realtime external exchanges position state. If external position changes from outside, DiffOrder is generated and put to RabbitMq exchange.
3. Consumes ExchangeConnector open trades and accounts trade data to track normal position changes.
4. Implement PositionControl big-loop >=1min that verifies position consistensy for all exchanges. That's required to handle situations when something went wrong and ExchangeConnector didn't get ExecutedTrade via WS or error happened and ExecutedTrade have not been placed to RabbitMq.