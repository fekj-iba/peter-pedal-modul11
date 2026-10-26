import { gql } from './graphql.js';

// The customer's own page. It only shows what the API returns: whether Lise may see case 1 is decided by
// the API (Query.RepairCase), never by this page.

// Danish labels for the GraphQL CaseStatus enum values.
const STATUS_LABELS = {
  CREATED: 'Oprettet',
  AWAITING_APPROVAL: 'Afventer godkendelse',
  APPROVED: 'Godkendt',
  FINISHED: 'Afsluttet',
  PAID: 'Betalt',
};

const $ = (id) => document.getElementById(id);

const formatPrice = (price) =>
  price === null ? '–' : new Intl.NumberFormat('da-DK', { minimumFractionDigits: 2 }).format(price) + ' kr';

function showMessage(text, isError = false) {
  $('message').textContent = text;
  $('message').className = isError ? 'message error' : 'message';
  $('message').hidden = false;
}

async function showCase(id) {
  const data = await gql(`query($id: Int!) { repairCase(id: $id) {
    frameNumber problem status price customer { firstName lastName phone } parts { name } } }`, { id });
  const repairCase = data.repairCase;
  if (repairCase === null) {
    showMessage('Sagen findes ikke.', true);
    return;
  }
  $('detail-title').textContent = repairCase.frameNumber;
  $('detail-customer').textContent = `${repairCase.customer.firstName} ${repairCase.customer.lastName}`;
  $('detail-phone').textContent = repairCase.customer.phone;
  $('detail-problem').textContent = repairCase.problem;
  $('detail-status').textContent = STATUS_LABELS[repairCase.status];
  $('detail-price').textContent = formatPrice(repairCase.price);
  $('detail-parts').replaceChildren(...repairCase.parts.map((part) => {
    const li = document.createElement('li');
    li.textContent = part.name;
    return li;
  }));
  $('detail').hidden = false;
}

// The case number is in the address (min-sag.html?id=2), so the page can be bookmarked – and edited.
$('case-form').addEventListener('submit', (event) => {
  event.preventDefault();
  window.location.search = `?id=${$('case-id').value}`;
});

// GraphQL's Int is a 32-bit number; anything else cannot be a case number.
const MAX_INT = 2147483647;
const idText = new URLSearchParams(window.location.search).get('id');
if (idText !== null) {
  const id = Number(idText);
  if (Number.isInteger(id) && id > 0 && id <= MAX_INT) {
    $('case-id').value = id;
    showCase(id).catch((error) => showMessage(error.message, true));
  } else {
    showMessage('Sagen findes ikke.', true);
  }
}
