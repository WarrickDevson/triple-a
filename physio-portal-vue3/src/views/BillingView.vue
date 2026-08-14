<script setup lang="ts">
import { computed, ref } from 'vue'
import {
  demoInvoices,
  formatCurrency,
  invoiceStatusClass,
  planFeatures,
  type InvoiceItem,
} from '../data/billingDemo'
import { useAuthStore } from '../store/auth'
import BaseButton from '../components/BaseButton.vue'

const auth = useAuthStore()
const invoices = ref<InvoiceItem[]>([...demoInvoices])

const selectedInvoiceForPay = ref<InvoiceItem | null>(null)
const selectedInvoiceForPdf = ref<InvoiceItem | null>(null)
const showManagePlanModal = ref(false)
const paymentMethod = ref('EFT / Direct Deposit')
const paymentSuccess = ref(false)

const outstandingBalance = computed(() =>
  invoices.value.filter((i) => i.status !== 'Paid').reduce((sum, i) => sum + i.amount, 0),
)
const paidThisMonth = computed(() =>
  invoices.value.filter((i) => i.status === 'Paid').reduce((sum, i) => sum + i.amount, 0),
)

const tier = auth.user?.subscriptionTier ?? 'Professional'
const features = planFeatures[tier] ?? planFeatures.Professional ?? planFeatures.Basic!

function handleSimulatePayment() {
  if (!selectedInvoiceForPay.value) return
  const inv = invoices.value.find((i) => i.id === selectedInvoiceForPay.value!.id)
  if (inv) {
    inv.status = 'Paid'
  }
  paymentSuccess.value = true
  setTimeout(() => {
    paymentSuccess.value = false
    selectedInvoiceForPay.value = null
  }, 1800)
}

function handleDownloadPdf(invoice: InvoiceItem) {
  selectedInvoiceForPdf.value = invoice
  const content = `=====================================================
INVOICE STATEMENT: ${invoice.invoiceNumber}
TRIPLE A VETERINARY PHYSIOTHERAPY
=====================================================

Invoice Number:   ${invoice.invoiceNumber}
Date:             ${invoice.date}
Patient:          ${invoice.petName}
Owner:            ${invoice.ownerName}
Status:           ${invoice.status}

-----------------------------------------------------
LINE ITEMS & CLINICAL SERVICES
-----------------------------------------------------
Veterinary Physiotherapy Consultation & Rehab Session
Subtotal:         ${formatCurrency(invoice.amount)}
VAT (15%):        Included
Total Amount:     ${formatCurrency(invoice.amount)}

-----------------------------------------------------
Thank you for choosing Triple A Veterinary Physiotherapy!
=====================================================
`
  const blob = new Blob([content], { type: 'text/plain;charset=utf-8' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `${invoice.invoiceNumber}.txt`
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}
</script>

<template>
  <div class="space-y-4">
    <!-- Stat Header -->
    <div class="grid gap-4 sm:grid-cols-3">
      <section class="portal-card p-4">
        <p class="text-xs font-semibold uppercase tracking-wide text-neutral-muted">Outstanding Balance</p>
        <p class="mt-1 text-2xl font-extrabold text-navy">{{ formatCurrency(outstandingBalance) }}</p>
      </section>
      <section class="portal-card p-4">
        <p class="text-xs font-semibold uppercase tracking-wide text-neutral-muted">Paid This Month</p>
        <p class="mt-1 text-2xl font-extrabold text-emerald-700">{{ formatCurrency(paidThisMonth) }}</p>
      </section>
      <section class="portal-card p-4">
        <p class="text-xs font-semibold uppercase tracking-wide text-neutral-muted">Next Payment Due</p>
        <p class="mt-1 text-2xl font-extrabold text-navy">
          {{ new Date().toLocaleDateString([], { day: 'numeric', month: 'short' }) }}
        </p>
      </section>
    </div>

    <div class="grid gap-4 xl:grid-cols-[minmax(0,1fr)_300px]">
      <!-- Invoices Table Card -->
      <section class="portal-card overflow-hidden">
        <div class="border-b border-neutral-grey/80 px-4 py-3 flex items-center justify-between">
          <h2 class="text-sm font-bold text-navy">Clinical Invoices</h2>
          <span class="text-xs text-neutral-muted">Billing Test Mode</span>
        </div>
        <div class="overflow-x-auto">
          <table class="w-full min-w-[600px] text-left text-sm">
            <thead>
              <tr class="border-b border-neutral-grey/80 text-xs font-semibold uppercase tracking-wide text-neutral-muted">
                <th class="px-4 py-3">Invoice #</th>
                <th class="px-4 py-3">Patient / Owner</th>
                <th class="px-4 py-3">Date</th>
                <th class="px-4 py-3">Amount</th>
                <th class="px-4 py-3">Status</th>
                <th class="px-4 py-3">Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="invoices.length === 0">
                <td colspan="6" class="p-8 text-center text-xs text-neutral-muted">
                  No invoices found.
                </td>
              </tr>
              <tr
                v-for="invoice in invoices"
                :key="invoice.id"
                class="border-b border-neutral-grey/60 hover:bg-surface/80 transition-colors"
              >
                <td class="px-4 py-3 font-bold text-navy">{{ invoice.invoiceNumber }}</td>
                <td class="px-4 py-3 text-neutral-muted">
                  <span class="font-bold text-navy">{{ invoice.petName }}</span><br />
                  <span class="text-xs">{{ invoice.ownerName }}</span>
                </td>
                <td class="px-4 py-3 text-neutral-muted">
                  {{ new Date(invoice.date).toLocaleDateString() }}
                </td>
                <td class="px-4 py-3 font-extrabold text-navy">{{ formatCurrency(invoice.amount) }}</td>
                <td class="px-4 py-3">
                  <span :class="invoiceStatusClass(invoice.status)">{{ invoice.status }}</span>
                </td>
                <td class="px-4 py-3">
                  <div class="flex gap-2">
                    <button
                      v-if="invoice.status !== 'Paid'"
                      type="button"
                      class="rounded bg-sage px-2.5 py-1 text-xs font-bold text-white hover:bg-navy transition-colors shadow-sm"
                      @click="selectedInvoiceForPay = invoice"
                    >
                      Record Payment
                    </button>
                    <button
                      type="button"
                      class="rounded border border-neutral-grey px-2.5 py-1 text-xs font-semibold text-navy hover:bg-surface transition-colors"
                      @click="handleDownloadPdf(invoice)"
                    >
                      PDF
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- Subscription Card -->
      <section class="portal-card p-5">
        <h3 class="text-sm font-bold text-navy">Clinic Subscription</h3>
        <p class="mt-2 text-xl font-extrabold text-sage">{{ tier }} Plan</p>
        <ul class="mt-4 space-y-2 text-xs text-neutral-muted">
          <li v-for="feature in features" :key="feature" class="flex items-start gap-2">
            <span class="mt-1 h-1.5 w-1.5 shrink-0 rounded-full bg-sage" />
            {{ feature }}
          </li>
        </ul>
        <button
          type="button"
          class="mt-5 text-xs font-bold text-sage hover:text-navy"
          @click="showManagePlanModal = true"
        >
          Manage Subscription & Tier →
        </button>
      </section>
    </div>

    <!-- Simulate Payment Modal -->
    <div
      v-if="selectedInvoiceForPay"
      class="fixed inset-0 z-50 flex items-center justify-center bg-navy/50 p-4 backdrop-blur-sm"
      @click.self="selectedInvoiceForPay = null"
    >
      <div class="portal-card w-full max-w-md p-6 shadow-xl">
        <h3 class="text-lg font-bold text-navy">Record Test Payment</h3>
        <p class="text-xs text-neutral-muted mt-0.5">
          Mark invoice {{ selectedInvoiceForPay.invoiceNumber }} for {{ selectedInvoiceForPay.petName }} as paid
        </p>

        <div v-if="paymentSuccess" class="mt-4 rounded-xl bg-emerald-50 p-4 text-center border border-emerald-200 text-emerald-900">
          <p class="font-extrabold text-sm">✓ Payment Recorded Successfully!</p>
          <p class="text-xs mt-1">Invoice updated to Paid state.</p>
        </div>

        <form v-else class="mt-4 space-y-4" @submit.prevent="handleSimulatePayment">
          <div class="rounded-xl bg-surface p-3 text-xs flex justify-between">
            <span class="text-neutral-muted">Total Invoice Amount:</span>
            <span class="font-extrabold text-navy text-sm">{{ formatCurrency(selectedInvoiceForPay.amount) }}</span>
          </div>

          <label class="block">
            <span class="text-xs font-semibold uppercase tracking-wider text-navy">Payment Method</span>
            <select
              v-model="paymentMethod"
              class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm bg-white focus:border-sage"
            >
              <option value="EFT / Direct Deposit">EFT / Direct Bank Deposit</option>
              <option value="Credit / Debit Card (Demo Gateway)">Credit / Debit Card (Demo Gateway)</option>
              <option value="Cash / POS Terminal">Cash / POS Terminal</option>
            </select>
          </label>

          <div class="flex gap-3 pt-2">
            <BaseButton type="button" variant="secondary" class="flex-1" @click="selectedInvoiceForPay = null">
              Cancel
            </BaseButton>
            <BaseButton type="submit" class="flex-1">Mark Paid</BaseButton>
          </div>
        </form>
      </div>
    </div>

    <!-- Manage Subscription Plan Modal -->
    <div
      v-if="showManagePlanModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-navy/50 p-4 backdrop-blur-sm"
      @click.self="showManagePlanModal = false"
    >
      <div class="portal-card w-full max-w-lg p-6 shadow-xl">
        <h3 class="text-lg font-bold text-navy">Manage Subscription Tier</h3>
        <p class="text-xs text-neutral-muted mt-0.5">Testing subscription plan configurations for your clinic</p>

        <div class="mt-4 grid gap-3 sm:grid-cols-3">
          <div class="rounded-xl border p-3 border-sage bg-sage/5">
            <p class="text-xs font-bold text-navy">Professional</p>
            <p class="text-lg font-extrabold text-sage mt-1">Active</p>
            <p class="text-[10px] text-neutral-muted mt-1">Unlimited patients & video processing</p>
          </div>
          <div class="rounded-xl border p-3 border-neutral-grey">
            <p class="text-xs font-bold text-navy">Basic</p>
            <p class="text-sm font-semibold text-neutral-muted mt-1">R499 / mo</p>
            <p class="text-[10px] text-neutral-muted mt-1">Up to 25 patients</p>
          </div>
          <div class="rounded-xl border p-3 border-neutral-grey">
            <p class="text-xs font-bold text-navy">Enterprise</p>
            <p class="text-sm font-semibold text-neutral-muted mt-1">Custom</p>
            <p class="text-[10px] text-neutral-muted mt-1">Multi-clinic & SLA</p>
          </div>
        </div>

        <div class="mt-6 flex justify-end">
          <BaseButton size="sm" @click="showManagePlanModal = false">Close</BaseButton>
        </div>
      </div>
    </div>
  </div>
</template>
