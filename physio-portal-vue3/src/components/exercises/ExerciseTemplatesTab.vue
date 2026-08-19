<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { Layers, ChevronRight, CheckCircle } from '@lucide/vue'
import BaseButton from '../BaseButton.vue'
import { usePatientsStore } from '../../store/patients'

const router = useRouter()
const patientsStore = usePatientsStore()

interface ProtocolTemplate {
  id: string
  title: string
  targetCondition: string
  species: string
  durationWeeks: string
  exerciseCount: number
  description: string
  exercises: string[]
}

const templates: ProtocolTemplate[] = [
  {
    id: 't1',
    title: 'Post-CCL Surgery Rehabilitation Protocol',
    targetCondition: 'Cranial Cruciate Ligament (CCL) Repair / TPLO',
    species: 'Canine',
    durationWeeks: '8-12 Weeks',
    exerciseCount: 6,
    description: 'Structured 4-phase protocol for post-TPLO or lateral suture recovery focusing on passive ROM, weight bearing, and quadriceps strength.',
    exercises: [
      'Passive Range of Motion (PROM) - Knee Flexion',
      'Controlled Leash Walks (5-10 mins)',
      'Sit-to-Stand Transitions (Squats)',
      'Weight Shifts on Balance Board',
      'Underwater Treadmill Hydrotherapy',
    ],
  },
  {
    id: 't2',
    title: 'Canine Hip Dysplasia & Arthritis Protocol',
    targetCondition: 'Hip Dysplasia & Osteoarthritis',
    species: 'Canine',
    durationWeeks: 'Ongoing / 6-Week Block',
    exerciseCount: 5,
    description: 'Low-impact muscle strengthening and joint mobilisation designed to stabilize hip joints and reduce stiffness.',
    exercises: [
      'Hindlimb Cookie Stretches',
      'Low Cavaletti Rail Walking',
      'Incline Hill Walks',
      'Targeted Gluteal Isometric Holds',
    ],
  },
  {
    id: 't3',
    title: 'Senior Canine Vitality & Balance Protocol',
    targetCondition: 'Age-Related Sarcopenia & Proprioceptive Deficits',
    species: 'Canine',
    durationWeeks: 'Ongoing',
    exerciseCount: 4,
    description: 'Gentle daily exercises to maintain joint flexibility, core balance, and neurological paw placement in senior dogs.',
    exercises: [
      'Paw Lifts on Foam Pad',
      'Figure-8 Weaving',
      'Gentle Warm Compression & Back Massage',
    ],
  },
  {
    id: 't4',
    title: 'Tendinopathy & Soft Tissue Strain Protocol',
    targetCondition: 'Biceps Tendinopathy & Muscle Strains',
    species: 'Canine & Feline',
    durationWeeks: '6 Weeks',
    exerciseCount: 5,
    description: 'Eccentric muscle loading and therapeutic laser protocol for tendon remodeling and inflammation management.',
    exercises: [
      'Eccentric Shoulder Flexion',
      'Therapeutic Laser Therapy (MLS)',
      'Slow Stepping Over Low Foam Obstacles',
    ],
  },
]

const selectedTemplate = ref<ProtocolTemplate | null>(null)
const showAssignModal = ref(false)
const selectedPetId = ref(patientsStore.patients[0]?.petId || 1)

function openAssignModal(template: ProtocolTemplate) {
  selectedTemplate.value = template
  showAssignModal.value = true
}

function handleApplyProtocol() {
  if (selectedPetId.value) {
    router.push({ name: 'treatment-plan-detail', params: { petId: selectedPetId.value } })
  }
}
</script>

<template>
  <div class="space-y-4">
    <div class="flex items-center justify-between">
      <div>
        <h3 class="text-sm font-bold text-navy">Pre-built Clinical Rehabilitation Protocols</h3>
        <p class="text-xs text-neutral-muted">
          Evidence-based exercise templates ready to prescribe to patient treatment plans
        </p>
      </div>
    </div>

    <!-- Templates Grid -->
    <div class="grid gap-4 sm:grid-cols-2">
      <div
        v-for="template in templates"
        :key="template.id"
        class="portal-card flex flex-col justify-between p-5 border border-neutral-grey/60 hover:border-sage/40 transition-all shadow-sm"
      >
        <div>
          <div class="flex items-start justify-between gap-2">
            <span class="rounded-md bg-sage-muted px-2.5 py-0.5 text-[10px] font-extrabold uppercase tracking-wider text-sage">
              {{ template.species }} · {{ template.durationWeeks }}
            </span>
            <span class="text-xs font-semibold text-neutral-muted flex items-center gap-1">
              <Layers class="h-3.5 w-3.5" />
              {{ template.exerciseCount }} Exercises
            </span>
          </div>

          <h4 class="text-base font-extrabold text-navy mt-2">{{ template.title }}</h4>
          <p class="text-xs text-sage font-semibold mt-0.5">{{ template.targetCondition }}</p>
          <p class="text-xs text-neutral-muted mt-2 leading-relaxed">{{ template.description }}</p>

          <div class="mt-4 rounded-xl bg-surface p-3 space-y-1.5">
            <p class="text-[10px] font-bold uppercase tracking-wider text-neutral-muted">Included Exercises</p>
            <ul class="space-y-1 text-xs text-navy/90">
              <li v-for="ex in template.exercises" :key="ex" class="flex items-center gap-1.5">
                <CheckCircle class="h-3.5 w-3.5 text-sage shrink-0" />
                <span class="truncate">{{ ex }}</span>
              </li>
            </ul>
          </div>
        </div>

        <div class="mt-5 flex items-center justify-between border-t border-neutral-grey/40 pt-3">
          <span class="text-xs text-neutral-muted">Ready to prescribe</span>
          <BaseButton size="sm" @click="openAssignModal(template)">
            Use Protocol
            <ChevronRight class="h-4 w-4" />
          </BaseButton>
        </div>
      </div>
    </div>

    <!-- Assign Modal -->
    <div
      v-if="showAssignModal && selectedTemplate"
      class="fixed inset-0 z-50 flex items-center justify-center bg-navy/50 p-4 backdrop-blur-sm"
      @click.self="showAssignModal = false"
    >
      <div class="portal-card w-full max-w-md p-6 shadow-xl">
        <h3 class="text-lg font-bold text-navy">Apply {{ selectedTemplate.title }}</h3>
        <p class="text-xs text-neutral-muted mt-0.5">Assign this rehabilitation protocol to a patient</p>

        <form class="mt-4 space-y-4" @submit.prevent="handleApplyProtocol">
          <label class="block">
            <span class="text-xs font-semibold uppercase tracking-wider text-navy">Select Patient</span>
            <select
              v-model="selectedPetId"
              class="mt-1 w-full rounded-lg border border-neutral-grey px-3 py-2 text-sm bg-white focus:border-sage"
            >
              <option v-for="p in patientsStore.patients" :key="p.petId" :value="p.petId">
                {{ p.petName }} ({{ p.ownerName }})
              </option>
            </select>
          </label>

          <div class="rounded-xl border border-sage/30 bg-sage-muted/20 p-3 text-xs text-navy">
            <p class="font-bold">Protocol Summary:</p>
            <p class="mt-1">{{ selectedTemplate.exerciseCount }} exercises will be added to the patient's treatment plan.</p>
          </div>

          <div class="flex gap-3 pt-2">
            <BaseButton type="button" variant="secondary" class="flex-1" @click="showAssignModal = false">
              Cancel
            </BaseButton>
            <BaseButton type="submit" class="flex-1">Apply Protocol</BaseButton>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>
