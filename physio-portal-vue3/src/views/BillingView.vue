<script setup lang="ts">
import { computed, ref } from 'vue'
import {
  formatCurrency,
  invoiceStatusClass,
  planFeatures,
  type InvoiceItem,
} from '../data/billingDemo'
import { useAuthStore } from '../store/auth'
import BaseButton from '../components/BaseButton.vue'

const auth = useAuthStore()
const showStubModal = ref(false)
const stubMessage = ref('')

const invoices = ref<InvoiceItem[]>([])

const outstandingBalance = computed(() =>
  invoices.value.filter((i) => i.status !== 'Paid').reduce((sum, i) => sum + i.amount, 0),
)
const paidThisMonth = computed(() =>
  invoices.value.filter((i) => i.status === 'Paid').reduce((sum, i) => sum + i.amount, 0),
)

const tier = auth.user?.subscriptionTier ?? 'Free'
const features = planFeatures[tier] ?? planFeatures.Free ?? planFeatures.Professional!

function showStub(message: string) {
  stubMessage.value = message
  showStubModal.value = true
}
</script>

<template>
  <div class="space-y-4">
    <div class="grid gap-4 sm:grid-cols-3">
      <section class="portal-card p-4">
        <p class="text-xs font-semibold uppercase tracking-wide text-neutral-muted">Outstanding</p>
        <p class="mt-1 text-2xl font-bold text-navy">{{ formatCurrency(outstandingBalance) }}</p>
      </section>
      <section class="portal-card p-4">
        <p class="text-xs font-semibold uppercase tracking-wide text-neutral-muted">Paid This Month</p>
        <p class="mt-1 text-2xl font-bold text-success-green">{{ formatCurrency(paidThisMonth) }}</p>
      </section>
      <section class="portal-card p-4">
        <p class="text-xs font-semibold uppercase tracking-wide text-neutral-muted">Next Payment Due</p>
        <p class="mt-1 text-2xl font-bold text-navy">
          {{ new Date().toLocaleDateString([], { day: 'numeric', month: 'short' }) }}
        </p>
      </section>
    </div>

    <div class="grid gap-4 xl:grid-cols-[minmax(0,1fr)_280px]">
      <section class="portal-card overflow-hidden">
        <div class="border-b border-neutral-grey/80 px-4 py-3">
          <h2 class="text-sm font-bold text-navy">Invoices</h2>
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
                class="border-b border-neutral-grey/60 hover:bg-surface"
              >
                <td class="px-4 py-3 font-medium text-navy">{{ invoice.invoiceNumber }}</td>
                <td class="px-4 py-3 text-neutral-muted">
                  {{ invoice.petName }}<br />
                  <span class="text-xs">{{ invoice.ownerName }}</span>
                </td>
                <td class="px-4 py-3 text-neutral-muted">
                  {{ new Date(invoice.date).toLocaleDateString() }}
                </td>
                <td class="px-4 py-3 font-semibold text-navy">{{ formatCurrency(invoice.amount) }}</td>
                <td class="px-4 py-3">
                  <span :class="invoiceStatusClass(invoice.status)">{{ invoice.status }}</span>
                </td>
                <td class="px-4 py-3">
                  <div class="flex gap-2">
                    <button
                      v-if="invoice.status !== 'Paid'"
                      type="button"
                      class="text-xs font-semibold text-sage hover:text-navy"
                      @click="showStub('Online payment coming soon.')"
                    >
                      Pay
                    </button>
                    <button
                      type="button"
                      class="text-xs font-semibold text-sage hover:text-navy"
                      @click="showStub('Invoice PDF download coming soon.')"
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

      <section class="portal-card p-4">
        <h3 class="text-sm font-bold text-navy">Subscription</h3>
        <p class="mt-2 text-lg font-bold text-sage">{{ tier }} Plan</p>
        <ul class="mt-4 space-y-2 text-sm text-neutral-muted">
          <li v-for="feature in features" :key="feature" class="flex items-start gap-2">
            <span class="mt-1 h-1.5 w-1.5 shrink-0 rounded-full bg-sage" />
            {{ feature }}
          </li>
        </ul>
        <button
          type="button"
          class="mt-4 text-sm font-semibold text-sage hover:text-navy"
          @click="showStub('Plan management coming soon.')"
        >
          Manage subscription →
        </button>
      </section>
    </div>
  </div>

  <div
    v-if="showStubModal"
    class="fixed inset-0 z-50 flex items-center justify-center bg-navy/50 p-4"
    @click.self="showStubModal = false"
  >
    <div class="portal-card max-w-sm p-6 text-center">
      <p class="text-sm text-neutral-muted">{{ stubMessage }}</p>
      <BaseButton class="mt-4" size="sm" @click="showStubModal = false">Close</BaseButton>
    </div>
  </div>
</template>
