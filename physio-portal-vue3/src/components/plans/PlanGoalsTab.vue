<script setup lang="ts">
import { computed, ref } from 'vue'
import { CheckCircle2, Circle, Plus, Target, Calendar } from '@lucide/vue'
import BaseButton from '../BaseButton.vue'
import type { PlanPhase } from '../../data/planDemo'
import type { Pet } from '../../types/pet'

const props = defineProps<{
  patient: Pet
  phases: PlanPhase[]
  activePhaseId: number
}>()

interface GoalItem {
  id: string
  phaseId: number
  title: string
  category: 'Mobility' | 'Strength' | 'Pain Management' | 'Function'
  targetWeek: string
  completed: boolean
}

// Initial goals synthesized from phases
const initialGoals: GoalItem[] = [
  { id: 'g1', phaseId: 1, title: 'Reduce resting joint inflammation and surgical swelling', category: 'Pain Management', targetWeek: 'Week 1', completed: true },
  { id: 'g2', phaseId: 1, title: 'Achieve comfortable passive flexion to 90 degrees', category: 'Mobility', targetWeek: 'Week 2', completed: true },
  { id: 'g3', phaseId: 1, title: 'Tolerate 5-minute leash walks without limping flare-ups', category: 'Function', targetWeek: 'Week 2-3', completed: false },
  { id: 'g4', phaseId: 2, title: 'Improve weight bearing during sit-to-stand transitions', category: 'Strength', targetWeek: 'Week 4', completed: false },
  { id: 'g5', phaseId: 2, title: 'Increase quadriceps and hamstring muscle mass by 5%', category: 'Strength', targetWeek: 'Week 5-6', completed: false },
  { id: 'g6', phaseId: 3, title: 'Complete 20-minute trotting on varied terrain smoothly', category: 'Function', targetWeek: 'Week 7-8', completed: false },
  { id: 'g7', phaseId: 4, title: 'Return to unrestricted off-leash play and agility exercises', category: 'Function', targetWeek: 'Week 9-12', completed: false },
]

const goals = ref<GoalItem[]>(initialGoals)
const selectedCategory = ref<string>('All')
const showAddGoalModal = ref(false)

const newGoalForm = ref({
  title: '',
  category: 'Mobility' as GoalItem['category'],
  phaseId: props.activePhaseId || 1,
  targetWeek: 'Week 3-4',
})

const categories = ['All', 'Mobility', 'Strength', 'Pain Management', 'Function']

const filteredGoals = computed(() => {
  if (selectedCategory.value === 'All') return goals.value
  return goals.value.filter((g) => g.category === selectedCategory.value)
})

const completedCount = computed(() => goals.value.filter((g) => g.completed).length)
const totalCount = computed(() => goals.value.length)
const progressPercentage = computed(() =>
  totalCount.value === 0 ? 0 : Math.round((completedCount.value / totalCount.value) * 100),
)

function toggleGoal(id: string) {
  const goal = goals.value.find((g) => g.id === id)
  if (goal) {
    goal.completed = !goal.completed
  }
}

function addGoal() {
  if (!newGoalForm.value.title.trim()) return
  goals.value.push({
    id: `g_${Date.now()}`,
    phaseId: newGoalForm.value.phaseId,
    title: newGoalForm.value.title.trim(),
    category: newGoalForm.value.category,
    targetWeek: newGoalForm.value.targetWeek,
    completed: false,
  })
  newGoalForm.value.title = ''
  showAddGoalModal.value = false
}

function getCategoryBadgeClass(category: GoalItem['category']) {
  switch (category) {
    case 'Mobility':
      return 'bg-blue-50 text-blue-700 border-blue-200'
    case 'Strength':
      return 'bg-purple-50 text-purple-700 border-purple-200'
    case 'Pain Management':
      return 'bg-amber-50 text-amber-800 border-amber-200'
    case 'Function':
      return 'bg-emerald-50 text-emerald-800 border-emerald-200'
    default:
      return 'bg-gray-50 text-gray-700 border-gray-200'
  }
}
</script>

<template>
  <div class="p-5 space-y-6">
    <!-- Header & Summary Cards -->
    <div class="grid gap-4 sm:grid-cols-3">
      <div class="portal-card p-4 flex items-center gap-4">
        <div class="flex h-12 w-12 shrink-0 items-center justify-center rounded-xl bg-sage-muted text-sage">
          <Target class="h-6 w-6" />
        </div>
        <div>
          <p class="text-xs font-semibold uppercase tracking-wider text-neutral-muted">Overall Completion</p>
          <p class="text-2xl font-extrabold text-navy mt-0.5">{{ progressPercentage }}%</p>
          <p class="text-xs text-neutral-muted">{{ completedCount }} of {{ totalCount }} goals met</p>
        </div>
      </div>

      <div class="portal-card p-4 flex items-center gap-4">
        <div class="flex h-12 w-12 shrink-0 items-center justify-center rounded-xl bg-emerald-50 text-emerald-700">
          <CheckCircle2 class="h-6 w-6" />
        </div>
        <div>
          <p class="text-xs font-semibold uppercase tracking-wider text-neutral-muted">Achieved Goals</p>
          <p class="text-2xl font-extrabold text-emerald-800 mt-0.5">{{ completedCount }}</p>
          <p class="text-xs text-neutral-muted">Milestones unlocked</p>
        </div>
      </div>

      <div class="portal-card p-4 flex items-center justify-center sm:justify-end">
        <BaseButton size="sm" @click="showAddGoalModal = true">
          <Plus class="h-4 w-4" />
          Add Custom Goal
        </BaseButton>
      </div>
    </div>

    <!-- Category Filter Bar -->
    <div class="flex items-center justify-between gap-3 border-b border-neutral-grey/60 pb-3 overflow-x-auto">
      <div class="flex gap-1">
        <button
          v-for="cat in categories"
          :key="cat"
          type="button"
          class="rounded-lg px-3 py-1.5 text-xs font-semibold transition-all"
          :class="
            selectedCategory === cat
              ? 'bg-navy text-white shadow-sm'
              : 'bg-surface text-neutral-muted hover:bg-neutral-grey/60 hover:text-navy'
          "
          @click="selectedCategory = cat"
        >
          {{ cat }}
        </button>
      </div>
      <span class="text-xs text-neutral-muted shrink-0">Showing {{ filteredGoals.length }} goals</span>
    </div>

    <!-- Goals List grouped by Phase -->
    <div class="space-y-6">
      <div
        v-for="phase in phases"
        :key="phase.id"
        class="portal-card overflow-hidden"
      >
        <div class="flex items-center justify-between border-b border-neutral-grey/60 bg-surface/50 px-4 py-3">
          <div class="flex items-center gap-2">
            <span class="rounded bg-sage-muted px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider text-sage">
              {{ phase.label }}
            </span>
            <h3 class="text-sm font-bold text-navy">{{ phase.title }}</h3>
          </div>
          <span class="text-xs font-semibold text-neutral-muted">
            {{ goals.filter((g) => g.phaseId === phase.id && g.completed).length }} /
            {{ goals.filter((g) => g.phaseId === phase.id).length }} Completed
          </span>
        </div>

        <div class="divide-y divide-neutral-grey/40 p-2">
          <div
            v-for="goal in filteredGoals.filter((g) => g.phaseId === phase.id)"
            :key="goal.id"
            class="flex items-center justify-between gap-4 p-3 hover:bg-surface/80 rounded-lg transition-colors cursor-pointer"
            @click="toggleGoal(goal.id)"
          >
            <div class="flex items-start gap-3 min-w-0 flex-1">
              <button
                type="button"
                class="mt-0.5 text-neutral-muted hover:text-sage transition-colors"
                @click.stop="toggleGoal(goal.id)"
              >
                <CheckCircle2 v-if="goal.completed" class="h-5 w-5 text-emerald-600 fill-emerald-100" />
                <Circle v-else class="h-5 w-5 text-neutral-muted" />
              </button>
              <div>
                <p
                  class="text-sm font-medium transition-all"
                  :class="goal.completed ? 'line-through text-neutral-muted' : 'text-navy'"
                >
                  {{ goal.title }}
                </p>
                <div class="flex items-center gap-2 mt-1">
                  <span
                    class="inline-block rounded-md border px-2 py-0.5 text-[10px] font-semibold"
                    :class="getCategoryBadgeClass(goal.category)"
                  >
                    {{ goal.category }}
                  </span>
                  <span class="flex items-center gap-1 text-[11px] text-neutral-muted">
                    <Calendar class="h-3 w-3" />
                    Target: {{ goal.targetWeek }}
                  </span>
                </div>
              </div>
            </div>

            <span
              class="rounded-full px-2.5 py-1 text-[10px] font-bold uppercase tracking-wider shrink-0"
              :class="goal.completed ? 'bg-emerald-100 text-emerald-800' : 'bg-amber-100 text-amber-800'"
            >
              {{ goal.completed ? 'Achieved' : 'In Progress' }}
            </span>
          </div>

          <div
            v-if="filteredGoals.filter((g) => g.phaseId === phase.id).length === 0"
            class="py-4 text-center text-xs text-neutral-muted"
          >
            No goals listed for this category in {{ phase.label }}.
          </div>
        </div>
      </div>
    </div>

    <!-- Add Goal Modal -->
    <div
      v-if="showAddGoalModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-navy/50 p-4"
      @click.self="showAddGoalModal = false"
    >
      <div class="portal-card w-full max-w-md p-6">
        <h3 class="text-lg font-bold text-navy">Add Rehabilitation Goal</h3>
        <p class="text-xs text-neutral-muted mt-0.5">Define a target milestone for {{ patient.petName }}</p>

        <form class="mt-4 space-y-4" @submit.prevent="addGoal">
          <label class="block">
            <span class="text-xs font-semibold uppercase tracking-wider text-navy">Goal Description</span>
            <input
              v-model="newGoalForm.title"
              required
              class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm focus:border-sage focus:outline-none"
              placeholder="e.g. Walk up 3 stairs without assistance"
            />
          </label>

          <div class="grid grid-cols-2 gap-3">
            <label class="block">
              <span class="text-xs font-semibold uppercase tracking-wider text-navy">Category</span>
              <select
                v-model="newGoalForm.category"
                class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm bg-white focus:border-sage"
              >
                <option value="Mobility">Mobility</option>
                <option value="Strength">Strength</option>
                <option value="Pain Management">Pain Management</option>
                <option value="Function">Function</option>
              </select>
            </label>

            <label class="block">
              <span class="text-xs font-semibold uppercase tracking-wider text-navy">Phase</span>
              <select
                v-model="newGoalForm.phaseId"
                class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm bg-white focus:border-sage"
              >
                <option v-for="p in phases" :key="p.id" :value="p.id">
                  {{ p.label }}
                </option>
              </select>
            </label>
          </div>

          <label class="block">
            <span class="text-xs font-semibold uppercase tracking-wider text-navy">Target Timeline</span>
            <input
              v-model="newGoalForm.targetWeek"
              required
              class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm focus:border-sage"
              placeholder="e.g. Week 3-4"
            />
          </label>

          <div class="flex gap-3 pt-2">
            <BaseButton type="button" variant="secondary" class="flex-1" @click="showAddGoalModal = false">
              Cancel
            </BaseButton>
            <BaseButton type="submit" class="flex-1">Add Goal</BaseButton>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>
