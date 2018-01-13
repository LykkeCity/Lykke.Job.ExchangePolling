# Exchange Polling Job

Stateful exchange polling job supplements Exchange Connector.

Service polls the list of exchanges to:
1. Get list of positions from exchanges that does not support websockets.
2. Control external exchange position state. If external position changes from outside, diff order must be generated.

Job initializes from current Hedging System position state to be confident that all changes are monitored.

All results are passed to special RabbitMq exchanges.