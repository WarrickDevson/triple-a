<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import {
  AlertTriangle,
  CheckCircle2,
  Clock,
  Mail,
  Plus,
  RefreshCw,
  Search,
  ShieldAlert,
  Trash2,
  UserCheck,
  UserX,
  Users,
} from '@lucide/vue'
import BaseButton from '../components/BaseButton.vue'
import BaseInput from '../components/BaseInput.vue'
import { useAuthStore } from '../store/auth'
import type { AdminUserSummary, PhysioApproval } from '../types/auth'

const auth = useAuthStore()

// Main section tab
const mainTab = ref<'physios' | 'deletion'>('physios')

// Physio approval state
const physios = ref<PhysioApproval[]>([])
const physiosLoading = ref(true)
const actionUserId = ref<number | null>(null)
const physioFilterTab = ref<'pending' | 'approved' | 'all'>('pending')
const physioSearchQuery = ref('')

// User data deletion state
const users = ref<AdminUserSummary[]>([])
const usersLoading = ref(false)
const userSearchQuery = ref('')
const userRoleFilter = ref<string>('')
const userStatusFilter = ref<'all' | 'active' | 'purged'>('all')

// Purge modal state
const showPurgeModal = ref(false)
const targetUser = ref<AdminUserSummary | null>(null)
const purgeMediaAndLogs = ref(true)
const purgeProcessing = ref(false)
const purgeSuccessMessage = ref<string | null>(null)

// Admin invite modal state
const showInviteModal = ref(false)
const inviteEmail = ref('')
const inviteClinicName = ref('')
const inviteSending = ref(false)

async function loadPhysios() {
  physiosLoading.value = true
  try {
    physios.value = await auth.fetchPendingPhysios()
  } finally {
    physiosLoading.value = false
  }
}

async function loadUsers() {
  usersLoading.value = true
  try {
    users.value = await auth.fetchAdminUsers(userSearchQuery.value.trim() || undefined, userRoleFilter.value || undefined)
  } finally {
    usersLoading.value = false
  }
}

onMounted(() => {
  loadPhysios()
})

watch(mainTab, (newTab) => {
  if (newTab === 'deletion' && users.value.length === 0) {
    loadUsers()
  }
})

// Debounced search for users
let searchTimeout: any = null
function onUserSearchInput() {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    loadUsers()
  }, 350)
}

function onUserRoleChange() {
  loadUsers()
}

// Filtered physios
const filteredPhysios = computed(() => {
  return physios.value.filter((p) => {
    const matchesSearch =
      `${p.firstName} ${p.lastName} ${p.email} ${p.clinicName || ''}`
        .toLowerCase()
        .includes(physioSearchQuery.value.toLowerCase())

    if (!matchesSearch) return false

    if (physioFilterTab.value === 'pending') {
      return !p.isApproved && p.isActive
    }
    if (physioFilterTab.value === 'approved') {
      return p.isApproved && p.isActive
    }
    return true
  })
})

const pendingCount = computed(() => physios.value.filter((p) => !p.isApproved && p.isActive).length)
const approvedCount = computed(() => physios.value.filter((p) => p.isApproved && p.isActive).length)
const totalPhysioCount = computed(() => physios.value.length)

// Filtered users for deletion tab
const filteredUsers = computed(() => {
  return users.value.filter((u) => {
    if (userStatusFilter.value === 'active') return u.isActive
    if (userStatusFilter.value === 'purged') return !u.isActive
    return true
  })
})

const totalUsersCount = computed(() => users.value.length)
const ownerUsersCount = computed(() => users.value.filter((u) => u.userRole === 'Owner').length)
const activeUsersCount = computed(() => users.value.filter((u) => u.isActive).length)
const purgedUsersCount = computed(() => users.value.filter((u) => !u.isActive).length)

// Actions
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

async function handleMarkEmailVerified(userId: number) {
  actionUserId.value = userId
  try {
    const ok = await auth.markEmailVerified(userId)
    if (ok) {
      const target = physios.value.find((p) => p.userId === userId)
      if (target) {
        target.isEmailVerified = true
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

// Purge action flow
function openPurgeModal(user: AdminUserSummary) {
  targetUser.value = user
  purgeMediaAndLogs.value = true
  purgeSuccessMessage.value = null
  showPurgeModal.value = true
}

async function handleConfirmPurge() {
  if (!targetUser.value) return
  purgeProcessing.value = true
  try {
    const ok = await auth.purgeUserData(targetUser.value.userId, {
      purgeMediaAndLogs: purgeMediaAndLogs.value,
    })
    if (ok) {
      purgeSuccessMessage.value = `User data for ${targetUser.value.email} was successfully purged.`
      await loadUsers()
      setTimeout(() => {
        showPurgeModal.value = false
        targetUser.value = null
      }, 1500)
    }
  } finally {
    purgeProcessing.value = false
  }
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header banner with main tabs -->
    <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between border-b border-neutral-grey pb-5">
      <div>
        <h1 class="text-2xl font-bold text-navy">Triple A SysAdmin Dashboard</h1>
        <p class="text-xs text-neutral-muted">
          Manage practitioner onboarding approvals, and process POPIA statutory user data deletion requests.
        </p>
      </div>

      <!-- Main Tab Switcher -->
      <div class="flex items-center rounded-xl bg-surface p-1 border border-neutral-grey">
        <button
          type="button"
          class="flex items-center gap-2 px-4 py-2 rounded-lg text-xs font-semibold transition-all"
          :class="mainTab === 'physios' ? 'bg-navy text-white shadow-sm' : 'text-neutral-muted hover:text-navy'"
          @click="mainTab = 'physios'"
        >
          <UserCheck class="h-4 w-4" />
          Physio Approvals
        </button>
        <button
          type="button"
          class="flex items-center gap-2 px-4 py-2 rounded-lg text-xs font-semibold transition-all"
          :class="mainTab === 'deletion' ? 'bg-red-700 text-white shadow-sm' : 'text-neutral-muted hover:text-navy'"
          @click="mainTab = 'deletion'"
        >
          <ShieldAlert class="h-4 w-4" />
          Data Deletion & Users
        </button>
      </div>
    </div>

    <!-- Alert / notification messages -->
    <div v-if="auth.message" class="rounded-xl border border-emerald-200 bg-emerald-50 p-3.5 text-xs text-emerald-800 font-medium">
      {{ auth.message }}
    </div>

    <!-- ========================================================================= -->
    <!-- TAB 1: PHYSIO APPROVALS                                                   -->
    <!-- ========================================================================= -->
    <template v-if="mainTab === 'physios'">
      <div class="flex items-center justify-between">
        <div class="flex items-center gap-2">
          <BaseButton variant="secondary" class="gap-2 text-xs" :disabled="physiosLoading" @click="loadPhysios">
            <RefreshCw class="h-3.5 w-3.5" :class="{ 'animate-spin': physiosLoading }" />
            Refresh
          </BaseButton>
        </div>

        <BaseButton variant="accent" class="gap-2 text-xs" @click="showInviteModal = true">
          <Plus class="h-4 w-4" />
          Send Admin Invite
        </BaseButton>
      </div>

      <!-- Physio Stat cards -->
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
          <p class="mt-3 text-3xl font-bold text-navy">{{ totalPhysioCount }}</p>
        </div>
      </div>

      <!-- Filter & search bar -->
      <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between rounded-2xl border border-neutral-grey bg-white p-4">
        <div class="flex items-center gap-2">
          <button
            type="button"
            class="px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors"
            :class="physioFilterTab === 'pending' ? 'bg-amber-100 text-amber-900 font-bold' : 'text-neutral-muted hover:bg-surface'"
            @click="physioFilterTab = 'pending'"
          >
            Pending Approval ({{ pendingCount }})
          </button>
          <button
            type="button"
            class="px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors"
            :class="physioFilterTab === 'approved' ? 'bg-emerald-100 text-emerald-900 font-bold' : 'text-neutral-muted hover:bg-surface'"
            @click="physioFilterTab = 'approved'"
          >
            Approved ({{ approvedCount }})
          </button>
          <button
            type="button"
            class="px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors"
            :class="physioFilterTab === 'all' ? 'bg-navy text-white' : 'text-neutral-muted hover:bg-surface'"
            @click="physioFilterTab = 'all'"
          >
            All ({{ totalPhysioCount }})
          </button>
        </div>

        <div class="relative w-full sm:w-64">
          <Search class="absolute left-3 top-2.5 h-4 w-4 text-neutral-muted" />
          <input
            v-model="physioSearchQuery"
            type="text"
            placeholder="Search name, email, clinic..."
            class="w-full rounded-xl border border-neutral-grey bg-surface py-2 pl-9 pr-3 text-xs text-navy placeholder:text-neutral-muted focus:border-sage focus:outline-none"
          />
        </div>
      </div>

      <!-- Physios table -->
      <div class="overflow-hidden rounded-2xl border border-neutral-grey bg-white shadow-sm">
        <div v-if="physiosLoading" class="p-8 text-center text-xs text-neutral-muted">
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
                <th class="px-5 py-3.5">Clinic Name</th>
                <th class="px-5 py-3.5">Email Status</th>
                <th class="px-5 py-3.5">Approval Status</th>
                <th class="px-5 py-3.5">Registered</th>
                <th class="px-5 py-3.5 text-right">Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-neutral-grey">
              <tr v-for="p in filteredPhysios" :key="p.userId" class="hover:bg-surface/50 transition-colors">
                <td class="px-5 py-4">
                  <div class="font-semibold text-navy">{{ p.firstName }} {{ p.lastName }}</div>
                  <div class="text-[11px] text-neutral-muted">{{ p.email }}</div>
                  <div v-if="p.phoneNumber" class="text-[11px] text-neutral-muted">{{ p.phoneNumber }}</div>
                </td>

                <td class="px-5 py-4">
                  <span v-if="p.clinicName" class="font-medium text-navy">{{ p.clinicName }}</span>
                  <span v-else class="text-neutral-muted italic">No clinic</span>
                </td>

                <td class="px-5 py-4">
                  <span
                    class="inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-[10px] font-semibold"
                    :class="p.isEmailVerified ? 'bg-emerald-50 text-emerald-700' : 'bg-amber-50 text-amber-700'"
                  >
                    <span class="h-1.5 w-1.5 rounded-full" :class="p.isEmailVerified ? 'bg-emerald-500' : 'bg-amber-500'" />
                    {{ p.isEmailVerified ? 'Verified' : 'Unverified' }}
                  </span>
                </td>

                <td class="px-5 py-4">
                  <span
                    class="inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-[10px] font-semibold"
                    :class="p.isApproved ? 'bg-emerald-50 text-emerald-700' : (!p.isActive ? 'bg-red-50 text-red-700' : 'bg-amber-50 text-amber-700')"
                  >
                    <span class="h-1.5 w-1.5 rounded-full" :class="p.isApproved ? 'bg-emerald-500' : (!p.isActive ? 'bg-red-500' : 'bg-amber-500')" />
                    {{ p.isApproved ? 'Approved' : (!p.isActive ? 'Rejected' : 'Pending Approval') }}
                  </span>
                </td>

                <td class="px-5 py-4 text-neutral-muted text-[11px]">
                  {{ new Date(p.createdDate).toLocaleDateString() }}
                </td>

                <td class="px-5 py-4 text-right">
                  <div class="flex items-center justify-end gap-2">
                    <BaseButton
                      v-if="!p.isEmailVerified"
                      variant="secondary"
                      class="h-8 px-3 text-[11px] gap-1 text-emerald-700 border-emerald-200 hover:bg-emerald-50"
                      :disabled="actionUserId === p.userId"
                      @click="handleMarkEmailVerified(p.userId)"
                    >
                      <CheckCircle2 class="h-3.5 w-3.5" />
                      Verify Email
                    </BaseButton>

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
    </template>

    <!-- ========================================================================= -->
    <!-- TAB 2: DATA DELETION & USER MANAGEMENT (POPIA SECTION 24)                 -->
    <!-- ========================================================================= -->
    <template v-else>
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h2 class="text-lg font-bold text-navy">POPIA User Data & Deletion Portal</h2>
          <p class="text-xs text-neutral-muted">
            Search registered users, execute statutory data erasure requests, and purge personal tracking records.
          </p>
        </div>

        <BaseButton variant="secondary" class="gap-2 text-xs" :disabled="usersLoading" @click="loadUsers">
          <RefreshCw class="h-3.5 w-3.5" :class="{ 'animate-spin': usersLoading }" />
          Refresh Users
        </BaseButton>
      </div>

      <!-- User Stat cards -->
      <div class="grid grid-cols-1 gap-4 sm:grid-cols-4">
        <div class="rounded-2xl border border-neutral-grey bg-white p-5 shadow-sm">
          <span class="text-xs font-semibold uppercase tracking-wider text-neutral-muted">Total Registered</span>
          <p class="mt-2 text-2xl font-bold text-navy">{{ totalUsersCount }}</p>
        </div>

        <div class="rounded-2xl border border-neutral-grey bg-white p-5 shadow-sm">
          <span class="text-xs font-semibold uppercase tracking-wider text-neutral-muted">Pet Owners</span>
          <p class="mt-2 text-2xl font-bold text-blue-600">{{ ownerUsersCount }}</p>
        </div>

        <div class="rounded-2xl border border-neutral-grey bg-white p-5 shadow-sm">
          <span class="text-xs font-semibold uppercase tracking-wider text-neutral-muted">Active Accounts</span>
          <p class="mt-2 text-2xl font-bold text-emerald-600">{{ activeUsersCount }}</p>
        </div>

        <div class="rounded-2xl border border-neutral-grey bg-white p-5 shadow-sm">
          <span class="text-xs font-semibold uppercase tracking-wider text-neutral-muted">Purged / Anonymized</span>
          <p class="mt-2 text-2xl font-bold text-neutral-muted">{{ purgedUsersCount }}</p>
        </div>
      </div>

      <!-- Filter & search bar for users -->
      <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between rounded-2xl border border-neutral-grey bg-white p-4">
        <div class="flex flex-wrap items-center gap-3">
          <div class="flex items-center gap-1 bg-surface p-1 rounded-xl border border-neutral-grey">
            <button
              type="button"
              class="px-3 py-1 rounded-lg text-xs font-semibold transition-colors"
              :class="userStatusFilter === 'all' ? 'bg-navy text-white' : 'text-neutral-muted hover:text-navy'"
              @click="userStatusFilter = 'all'"
            >
              All Status
            </button>
            <button
              type="button"
              class="px-3 py-1 rounded-lg text-xs font-semibold transition-colors"
              :class="userStatusFilter === 'active' ? 'bg-emerald-600 text-white' : 'text-neutral-muted hover:text-navy'"
              @click="userStatusFilter = 'active'"
            >
              Active
            </button>
            <button
              type="button"
              class="px-3 py-1 rounded-lg text-xs font-semibold transition-colors"
              :class="userStatusFilter === 'purged' ? 'bg-neutral-600 text-white' : 'text-neutral-muted hover:text-navy'"
              @click="userStatusFilter = 'purged'"
            >
              Purged
            </button>
          </div>

          <select
            v-model="userRoleFilter"
            class="rounded-xl border border-neutral-grey bg-surface py-1.5 px-3 text-xs text-navy focus:border-sage focus:outline-none"
            @change="onUserRoleChange"
          >
            <option value="">All Roles</option>
            <option value="Owner">Pet Owners</option>
            <option value="Physio">Physiotherapists</option>
          </select>
        </div>

        <div class="relative w-full sm:w-72">
          <Search class="absolute left-3 top-2.5 h-4 w-4 text-neutral-muted" />
          <input
            v-model="userSearchQuery"
            type="text"
            placeholder="Search email or name..."
            class="w-full rounded-xl border border-neutral-grey bg-surface py-2 pl-9 pr-3 text-xs text-navy placeholder:text-neutral-muted focus:border-sage focus:outline-none"
            @input="onUserSearchInput"
          />
        </div>
      </div>

      <!-- Users Deletion Management Table -->
      <div class="overflow-hidden rounded-2xl border border-neutral-grey bg-white shadow-sm">
        <div v-if="usersLoading" class="p-8 text-center text-xs text-neutral-muted">
          Loading user records...
        </div>

        <div v-else-if="filteredUsers.length === 0" class="p-8 text-center text-xs text-neutral-muted">
          No users match the search criteria.
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-left text-xs">
            <thead class="bg-surface text-neutral-muted border-b border-neutral-grey font-semibold">
              <tr>
                <th class="px-5 py-3.5">User Identity</th>
                <th class="px-5 py-3.5">Role</th>
                <th class="px-5 py-3.5">Associated Clinic</th>
                <th class="px-5 py-3.5">Pets</th>
                <th class="px-5 py-3.5">Status</th>
                <th class="px-5 py-3.5">Registered</th>
                <th class="px-5 py-3.5 text-right">Data Deletion</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-neutral-grey">
              <tr v-for="u in filteredUsers" :key="u.userId" class="hover:bg-surface/50 transition-colors">
                <td class="px-5 py-4">
                  <div class="font-semibold text-navy">{{ u.firstName }} {{ u.lastName }}</div>
                  <div class="text-[11px] text-neutral-muted font-mono">{{ u.email }}</div>
                  <div v-if="u.phoneNumber" class="text-[11px] text-neutral-muted">{{ u.phoneNumber }}</div>
                </td>

                <td class="px-5 py-4">
                  <span
                    class="inline-flex items-center rounded-full px-2.5 py-0.5 text-[10px] font-semibold"
                    :class="u.userRole === 'Owner' ? 'bg-blue-50 text-blue-700' : 'bg-emerald-50 text-emerald-700'"
                  >
                    {{ u.userRole }}
                  </span>
                </td>

                <td class="px-5 py-4">
                  <span v-if="u.clinicName" class="font-medium text-navy">{{ u.clinicName }}</span>
                  <span v-else class="text-neutral-muted italic">—</span>
                </td>

                <td class="px-5 py-4">
                  <span class="font-semibold text-navy">{{ u.petCount }}</span>
                </td>

                <td class="px-5 py-4">
                  <span
                    class="inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-[10px] font-semibold"
                    :class="u.isActive ? 'bg-emerald-50 text-emerald-700' : 'bg-neutral-100 text-neutral-600'"
                  >
                    <span class="h-1.5 w-1.5 rounded-full" :class="u.isActive ? 'bg-emerald-500' : 'bg-neutral-400'" />
                    {{ u.isActive ? 'Active' : 'Purged / Inactive' }}
                  </span>
                </td>

                <td class="px-5 py-4 text-neutral-muted text-[11px]">
                  {{ new Date(u.createdDate).toLocaleDateString() }}
                </td>

                <td class="px-5 py-4 text-right">
                  <BaseButton
                    v-if="u.isActive"
                    variant="secondary"
                    class="h-8 px-3 text-[11px] text-red-600 border-red-200 hover:bg-red-50 hover:border-red-300 gap-1.5 font-semibold"
                    @click="openPurgeModal(u)"
                  >
                    <Trash2 class="h-3.5 w-3.5" />
                    Purge User Data
                  </BaseButton>
                  <span v-else class="text-[11px] font-semibold text-neutral-400 italic">
                    Already Purged
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </template>

    <!-- ========================================================================= -->
    <!-- MODALS                                                                    -->
    <!-- ========================================================================= -->

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
            placeholder="e.g. Triple A Partner Clinic"
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

    <!-- POPIA Data Deletion / Purge Modal -->
    <div v-if="showPurgeModal && targetUser" class="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
      <div class="w-full max-w-lg rounded-2xl bg-white p-6 shadow-2xl border border-red-200 space-y-4">
        <div class="flex items-start justify-between">
          <div class="flex items-center gap-3">
            <div class="flex h-10 w-10 items-center justify-center rounded-xl bg-red-100 text-red-600">
              <AlertTriangle class="h-5 w-5" />
            </div>
            <div>
              <h3 class="text-base font-bold text-navy">Execute POPIA Data Deletion</h3>
              <p class="text-xs text-neutral-muted">Permanent user data erasure under POPIA Section 24</p>
            </div>
          </div>
          <button type="button" class="text-neutral-muted hover:text-navy text-sm font-bold" @click="showPurgeModal = false">
            ✕
          </button>
        </div>

        <div v-if="purgeSuccessMessage" class="rounded-xl bg-emerald-50 border border-emerald-200 p-3.5 text-xs text-emerald-800 font-semibold">
          {{ purgeSuccessMessage }}
        </div>

        <div class="rounded-xl bg-surface p-4 border border-neutral-grey space-y-2 text-xs">
          <div class="flex justify-between">
            <span class="text-neutral-muted">Target User:</span>
            <span class="font-bold text-navy">{{ targetUser.firstName }} {{ targetUser.lastName }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-neutral-muted">Email:</span>
            <span class="font-mono text-navy font-semibold">{{ targetUser.email }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-neutral-muted">Role & Pets:</span>
            <span class="font-semibold text-navy">{{ targetUser.userRole }} ({{ targetUser.petCount }} pets)</span>
          </div>
        </div>

        <div class="space-y-3 text-xs text-neutral-800">
          <p class="font-semibold text-red-700">
            Are you sure you want to permanently purge this user's data?
          </p>
          <ul class="list-disc pl-4 space-y-1 text-neutral-muted text-[11px]">
            <li>Personal credentials (name, email, phone, passwords) will be anonymized.</li>
            <li>All active sessions and refresh tokens will be immediately revoked.</li>
            <li>Statutory veterinary notes (SOAP notes) remain preserved for clinic compliance.</li>
          </ul>

          <label class="flex items-start gap-2.5 p-3 rounded-xl bg-red-50 border border-red-100 cursor-pointer">
            <input
              v-model="purgeMediaAndLogs"
              type="checkbox"
              class="mt-0.5 rounded text-red-600 focus:ring-red-500"
            />
            <span class="text-[11px] font-medium text-red-900 leading-tight">
              Permanently delete daily tracking logs and uploaded exercise form videos.
            </span>
          </label>
        </div>

        <div class="flex items-center justify-end gap-3 pt-2">
          <BaseButton
            type="button"
            variant="secondary"
            class="h-10 text-xs"
            :disabled="purgeProcessing"
            @click="showPurgeModal = false"
          >
            Cancel
          </BaseButton>

          <button
            type="button"
            class="inline-flex items-center justify-center gap-2 rounded-xl bg-red-600 px-4 py-2 text-xs font-bold text-white shadow-sm hover:bg-red-700 transition-colors disabled:opacity-50"
            :disabled="purgeProcessing"
            @click="handleConfirmPurge"
          >
            <Trash2 class="h-4 w-4" :class="{ 'animate-spin': purgeProcessing }" />
            {{ purgeProcessing ? 'Purging Data...' : 'Confirm & Purge Data' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
