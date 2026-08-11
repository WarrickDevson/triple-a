<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import {
  CheckCircle2,
  Clock,
  Mail,
  Plus,
  RefreshCw,
  Search,
  UserCheck,
  UserX,
  Users,
} from '@lucide/vue'
import BaseButton from '../components/BaseButton.vue'
import BaseInput from '../components/BaseInput.vue'
import { useAuthStore } from '../store/auth'
import type { PhysioApproval } from '../types/auth'

const auth = useAuthStore()

const physios = ref<PhysioApproval[]>([])
const loading = ref(true)
const actionUserId = ref<number | null>(null)
const filterTab = ref<'pending' | 'approved' | 'all'>('pending')
const searchQuery = ref('')

// Admin invite modal state
const showInviteModal = ref(false)
const inviteEmail = ref('')
const inviteClinicName = ref('')
const inviteSending = ref(false)

async function loadPhysios() {
  loading.value = true
  try {
    physios.value = await auth.fetchPendingPhysios()
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadPhysios()
})

const filteredPhysios = computed(() => {
  return physios.value.filter((p) => {
    const matchesSearch =
      `${p.firstName} ${p.lastName} ${p.email} ${p.clinicName || ''}`
        .toLowerCase()
        .includes(searchQuery.value.toLowerCase())

    if (!matchesSearch) return false

    if (filterTab.value === 'pending') {
      return !p.isApproved && p.isActive
    }
    if (filterTab.value === 'approved') {
      return p.isApproved && p.isActive
    }
    return true
  })
})

const pendingCount = computed(() => physios.value.filter((p) => !p.isApproved && p.isActive).length)
const approvedCount = computed(() => physios.value.filter((p) => p.isApproved && p.isActive).length)
const totalCount = computed(() => physios.value.length)

async function handleApprove(userId: number) {
  actionUserId.value = userId
  try {
    const ok = await auth.approvePhysio(userId)
    if (ok) {
      const target = physios.value.find((p) => p.userId === userId)
      if (target) {
        target.isApproved = true
        target.isActive = true
      }
    }
  } finally {
    actionUserId.value = null
  }
}

async function handleReject(userId: number) {
  if (!confirm('Are you sure you want to reject this physio registration?')) return
  actionUserId.value = userId
  try {
    const ok = await auth.rejectPhysio(userId)
    if (ok) {
      const target = physios.value.find((p) => p.userId === userId)
      if (target) {
        target.isApproved = false
        target.isActive = false
      }
    }
  } finally {
    actionUserId.value = null
  }
}

async function handleSendInvite() {
  if (!inviteEmail.value.trim()) return
  inviteSending.value = true
  try {
    const ok = await auth.sendAdminInvite(inviteEmail.value.trim(), inviteClinicName.value.trim() || undefined)
    if (ok) {
      showInviteModal.value = false
      inviteEmail.value = ''
      inviteClinicName.value = ''
      await loadPhysios()
    }
  } finally {
    inviteSending.value = false
  }
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header banner -->
    <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h1 class="text-2xl font-bold text-navy">SysAdmin Physio Management</h1>
        <p class="text-xs text-neutral-muted">
          Review pending registrations, approve practitioner accounts, and issue direct admin invites.
        </p>
      </div>

      <div class="flex items-center gap-3">
        <BaseButton variant="secondary" class="gap-2 text-xs" :disabled="loading" @click="loadPhysios">
          <RefreshCw class="h-3.5 w-3.5" :class="{ 'animate-spin': loading }" />
          Refresh
        </BaseButton>

        <BaseButton variant="accent" class="gap-2 text-xs" @click="showInviteModal = true">
          <Plus class="h-4 w-4" />
          Send Admin Invite
        </BaseButton>
      </div>
    </div>

    <!-- Alert / notification messages -->
    <div v-if="auth.message" class="rounded-xl border border-emerald-200 bg-emerald-50 p-3.5 text-xs text-emerald-800 font-medium">
      {{ auth.message }}
    </div>

    <!-- Stat cards -->
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-3">
      <div class="rounded-2xl border border-neutral-grey bg-white p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <span class="text-xs font-semibold uppercase tracking-wider text-neutral-muted">Pending Approval</span>
          <div class="flex h-9 w-9 items-center justify-center rounded-xl bg-amber-50 text-amber-600">
            <Clock class="h-5 w-5" />
          </div>
        </div>
        <p class="mt-3 text-3xl font-bold text-navy">{{ pendingCount }}</p>
      </div>

      <div class="rounded-2xl border border-neutral-grey bg-white p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <span class="text-xs font-semibold uppercase tracking-wider text-neutral-muted">Approved Physios</span>
          <div class="flex h-9 w-9 items-center justify-center rounded-xl bg-emerald-50 text-emerald-600">
            <UserCheck class="h-5 w-5" />
          </div>
        </div>
        <p class="mt-3 text-3xl font-bold text-navy">{{ approvedCount }}</p>
      </div>

      <div class="rounded-2xl border border-neutral-grey bg-white p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <span class="text-xs font-semibold uppercase tracking-wider text-neutral-muted">Total Practitioners</span>
          <div class="flex h-9 w-9 items-center justify-center rounded-xl bg-blue-50 text-blue-600">
            <Users class="h-5 w-5" />
          </div>
        </div>
        <p class="mt-3 text-3xl font-bold text-navy">{{ totalCount }}</p>
      </div>
    </div>

    <!-- Filter & search bar -->
    <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between rounded-2xl border border-neutral-grey bg-white p-4">
      <div class="flex items-center gap-2">
        <button
          type="button"
          class="px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors"
          :class="filterTab === 'pending' ? 'bg-amber-100 text-amber-900 font-bold' : 'text-neutral-muted hover:bg-surface'"
          @click="filterTab = 'pending'"
        >
          Pending Approval ({{ pendingCount }})
        </button>
        <button
          type="button"
          class="px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors"
          :class="filterTab === 'approved' ? 'bg-emerald-100 text-emerald-900 font-bold' : 'text-neutral-muted hover:bg-surface'"
          @click="filterTab = 'approved'"
        >
          Approved ({{ approvedCount }})
        </button>
        <button
          type="button"
          class="px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors"
          :class="filterTab === 'all' ? 'bg-navy text-white' : 'text-neutral-muted hover:bg-surface'"
          @click="filterTab = 'all'"
        >
          All ({{ totalCount }})
        </button>
      </div>

      <div class="relative w-full sm:w-64">
        <Search class="absolute left-3 top-2.5 h-4 w-4 text-neutral-muted" />
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Search name, email, clinic..."
          class="w-full rounded-xl border border-neutral-grey bg-surface py-2 pl-9 pr-3 text-xs text-navy placeholder:text-neutral-muted focus:border-sage focus:outline-none"
        />
      </div>
    </div>

    <!-- Physios table -->
    <div class="overflow-hidden rounded-2xl border border-neutral-grey bg-white shadow-sm">
      <div v-if="loading" class="p-8 text-center text-xs text-neutral-muted">
        Loading practitioner registrations...
      </div>

      <div v-else-if="filteredPhysios.length === 0" class="p-8 text-center text-xs text-neutral-muted">
        No physio sign-ups match the selected filter.
      </div>

      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-surface text-neutral-muted border-b border-neutral-grey font-semibold">
            <tr>
              <th class="px-5 py-3.5">Practitioner</th>
              <th class="px-5 py-3.5">Clinic</th>
              <th class="px-5 py-3.5">Email Status</th>
              <th class="px-5 py-3.5">Approval Status</th>
              <th class="px-5 py-3.5">Registered Date</th>
              <th class="px-5 py-3.5 text-right">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-neutral-grey text-navy">
            <tr v-for="p in filteredPhysios" :key="p.userId" class="hover:bg-surface/50 transition-colors">
              <td class="px-5 py-4">
                <div class="font-bold text-navy text-sm">{{ p.firstName }} {{ p.lastName }}</div>
                <div class="text-neutral-muted text-[11px]">{{ p.email }}</div>
                <div v-if="p.phoneNumber" class="text-neutral-muted text-[11px]">{{ p.phoneNumber }}</div>
              </td>
              <td class="px-5 py-4 font-medium">
                {{ p.clinicName || 'Personal Clinic' }}
              </td>
              <td class="px-5 py-4">
                <span
                  class="inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-[11px] font-semibold"
                  :class="p.isEmailVerified ? 'bg-emerald-50 text-emerald-700 border border-emerald-200' : 'bg-amber-50 text-amber-700 border border-amber-200'"
                >
                  <CheckCircle2 v-if="p.isEmailVerified" class="h-3 w-3" />
                  <Clock v-else class="h-3 w-3" />
                  {{ p.isEmailVerified ? 'Verified' : 'Unverified' }}
                </span>
              </td>
              <td class="px-5 py-4">
                <span
                  class="inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-[11px] font-semibold"
                  :class="p.isApproved ? 'bg-emerald-50 text-emerald-700 border border-emerald-200' : 'bg-amber-50 text-amber-700 border border-amber-200'"
                >
                  <UserCheck v-if="p.isApproved" class="h-3 w-3" />
                  <Clock v-else class="h-3 w-3" />
                  {{ p.isApproved ? 'Approved' : 'Pending Review' }}
                </span>
              </td>
              <td class="px-5 py-4 text-neutral-muted">
                {{ new Date(p.createdDate).toLocaleDateString() }}
              </td>
              <td class="px-5 py-4 text-right">
                <div class="flex items-center justify-end gap-2">
                  <BaseButton
                    v-if="!p.isApproved && p.isActive"
                    variant="accent"
                    class="h-8 px-3 text-[11px] gap-1"
                    :disabled="actionUserId === p.userId"
                    @click="handleApprove(p.userId)"
                  >
                    <UserCheck class="h-3.5 w-3.5" />
                    Approve
                  </BaseButton>

                  <BaseButton
                    v-if="p.isActive"
                    variant="secondary"
                    class="h-8 px-3 text-[11px] text-red-600 border-red-200 hover:bg-red-50 gap-1"
                    :disabled="actionUserId === p.userId"
                    @click="handleReject(p.userId)"
                  >
                    <UserX class="h-3.5 w-3.5" />
                    Reject
                  </BaseButton>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Admin Invite Modal -->
    <div v-if="showInviteModal" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">
      <div class="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl border border-neutral-grey space-y-4">
        <div class="flex items-center justify-between">
          <h3 class="text-lg font-bold text-navy">Send Admin Physio Invite</h3>
          <button type="button" class="text-neutral-muted hover:text-navy text-sm font-bold" @click="showInviteModal = false">
            ✕
          </button>
        </div>

        <p class="text-xs text-neutral-muted">
          Generate a direct invite link sent to the practitioner's email. Invited physios are automatically approved upon registration.
        </p>

        <form class="space-y-4" @submit.prevent="handleSendInvite">
          <BaseInput
            id="inviteEmail"
            v-model="inviteEmail"
            label="Practitioner Email"
            type="email"
            required
          />

          <BaseInput
            id="inviteClinicName"
            v-model="inviteClinicName"
            label="Clinic Name (Optional)"
            placeholder="e.g. MoveWell Partner Clinic"
          />

          <div class="flex items-center justify-end gap-3 pt-2">
            <BaseButton type="button" variant="secondary" class="h-10 text-xs" @click="showInviteModal = false">
              Cancel
            </BaseButton>

            <BaseButton type="submit" variant="accent" class="h-10 text-xs gap-1.5" :disabled="inviteSending || !inviteEmail.trim()">
              <Mail class="h-4 w-4" />
              {{ inviteSending ? 'Sending...' : 'Send Invite Link' }}
            </BaseButton>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>
