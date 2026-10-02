
#!/usr/bin/env bash
set -e

HOST=${HOST:-http://localhost:5118}
JSON="Content-Type: application/json"

echo "== create accounts =="
RAM=$(curl -s -X POST $HOST/api/accounts -H "$JSON" -d '{"owner":"Ram","currency":"NPR"}' | jq -r .id)
SITA=$(curl -s -X POST $HOST/api/accounts -H "$JSON" -d '{"owner":"Sita","currency":"NPR"}' | jq -r .id)
echo "Ram=$RAM"
echo "Sita=$SITA"

echo; echo "== deposit 10000 to Ram =="
curl -s -X POST $HOST/api/accounts/$RAM/deposit -H "$JSON" -d '{"amount":10000,"currency":"NPR"}' | jq

echo; echo "== transfer 4000 Ram -> Sita (expect 204) =="
curl -s -o /dev/null -w "HTTP %{http_code}\n" -X POST $HOST/api/transfers -H "$JSON" \
  -d "{\"sourceAccountId\":\"$RAM\",\"destinationAccountId\":\"$SITA\",\"amount\":4000,\"currency\":\"NPR\"}"

echo; echo "== balances (expect Ram 6000, Sita 4000) =="
curl -s $HOST/api/accounts/$RAM  | jq '{owner, balance}'
curl -s $HOST/api/accounts/$SITA | jq '{owner, balance}'

echo; echo "== Ram history =="
curl -s $HOST/api/accounts/$RAM/transactions | jq

echo; echo "== failure cases =="
printf "withdraw too much (expect 422):  "
curl -s -o /dev/null -w "HTTP %{http_code}\n" -X POST $HOST/api/accounts/$RAM/withdraw -H "$JSON" -d '{"amount":999999,"currency":"NPR"}'

printf "currency mismatch (expect 400):  "
curl -s -o /dev/null -w "HTTP %{http_code}\n" -X POST $HOST/api/accounts/$RAM/deposit -H "$JSON" -d '{"amount":100,"currency":"USD"}'

printf "same-account transfer (expect 400): "
curl -s -o /dev/null -w "HTTP %{http_code}\n" -X POST $HOST/api/transfers -H "$JSON" \
  -d "{\"sourceAccountId\":\"$RAM\",\"destinationAccountId\":\"$RAM\",\"amount\":10,\"currency\":\"NPR\"}"

printf "unknown account (expect 404):    "
curl -s -o /dev/null -w "HTTP %{http_code}\n" $HOST/api/accounts/00000000-0000-0000-0000-000000000001


