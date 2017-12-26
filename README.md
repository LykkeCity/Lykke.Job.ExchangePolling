# Exchange Polling Job

Stateful exchange polling job supplements Exchange Connector.

Service polls the list of exchanges to:
1. Get list of positions and accounts from exchanges that does not support websockets.
2. Get IsAlive status from all the exchanges and provide this data to consumers.
3. Control external exchange position state. If external position changes from outside, diff order must be generated.

All results are passed to special RabbitMq exchanges.