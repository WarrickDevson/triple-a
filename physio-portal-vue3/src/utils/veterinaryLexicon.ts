import type { VocabularyCategory } from '../types/soap'

export const VETERINARY_AUTO_CORRECTIONS: Record<string, string> = {
  'tea play low': 'TPLO',
  't p l o': 'TPLO',
  't-p-l-o': 'TPLO',
  't play lo': 'TPLO',
  'tea blow': 'TPLO',
  'pro m': 'PROM',
  'p r o m': 'PROM',
  'p-r-o-m': 'PROM',
  'arom': 'AROM',
  'a r o m': 'AROM',
  'u w t m': 'UWTM',
  'under water treadmill': 'underwater treadmill (UWTM)',
  'stiffle': 'stifle',
  'stiff-el': 'stifle',
  'stiffel': 'stifle',
  'ccl': 'CCL',
  'c-c-l': 'CCL',
  'patella lux': 'patellar luxation',
  'luxating patella': 'patellar luxation',
  'coxofemoral': 'coxofemoral',
  'ill you so as': 'iliopsoas',
  'ilio psoas': 'iliopsoas',
  'ivdd': 'IVDD',
  'i v d d': 'IVDD',
  'airex': 'Airex balance disc',
  'proprioception': 'proprioception',
  'for jewels': '4 J/cm²',
  'joules per centimeter': 'J/cm²',
  'joules per cm squared': 'J/cm²',
  'joules per cm2': 'J/cm²',
  'myofascial': 'myofascial',
  'goniometry': 'goniometry',
  'goniometer': 'goniometer',
  'nsaids': 'NSAIDs',
  'n-saids': 'NSAIDs',
  'meloxicam': 'meloxicam',
  'gabapentin': 'gabapentin',
  'carprofen': 'carprofen',
  'trochanter': 'greater trochanter',
  'lumbosacral': 'lumbosacral',
  'cervicothoracic': 'cervicothoracic',
}

export const VETERINARY_CATEGORIES: VocabularyCategory[] = [
  {
    category: 'Anatomy & Musculoskeletal',
    terms: [
      'Stifle', 'Patella', 'Coxofemoral Joint', 'Carpus', 'Tarsus / Hock',
      'Cranial Cruciate Ligament (CCL)', 'Iliopsoas', 'Lumbosacral Spine',
      'Biceps Femoris', 'Gastrocnemius', 'Quadriceps', 'Superficial Digital Flexor'
    ]
  },
  {
    category: 'Pathologies & Conditions',
    terms: [
      'TPLO Post-Op', 'Patellar Luxation Grade 1-4', 'Osteoarthritis (OA)',
      'Intervertebral Disc Disease (IVDD)', 'Hip Dysplasia', 'Elbow Dysplasia',
      'Degenerative Myelopathy', 'Muscle Strain / Contracture', 'Spondylosis'
    ]
  },
  {
    category: 'Modalities & Treatments',
    terms: [
      'Passive Range of Motion (PROM)', 'Active Range of Motion (AROM)',
      'Underwater Treadmill (UWTM)', 'Laser Therapy / PBMT',
      'Myofascial Release', 'Trigger Point Dry Needling', 'Therapeutic Ultrasound',
      'Cryotherapy / Cold Pack', 'Thermotherapy', 'TENS'
    ]
  },
  {
    category: 'Rehab Exercises',
    terms: [
      'Cavaletti Rails Walkover', 'Airex Balance Disc Standing', 'Sit-to-Stand Squats',
      'Three-Leg Standing / Weight Shift', 'Target Touch / Nose Touches',
      'Incline / Decline Ramp Walking', 'Backing Up Exercises', 'Figure 8 Weaves'
    ]
  },
  {
    category: 'Outcome Measures',
    terms: [
      'Goniometric ROM (deg)', 'Thigh Circumference Girth (cm)', 'Gait Lameness Grade (0-5)',
      'Palpation Soreness Score (0-10)', 'Morning Stiffness Score (0-10)', 'Proprioceptive Placing Response'
    ]
  }
]

export function correctVeterinaryTranscript(text: string): string {
  if (!text) return ''
  let result = text
  for (const [misheard, corrected] of Object.entries(VETERINARY_AUTO_CORRECTIONS)) {
    const regex = new RegExp(`\\b${misheard.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}\\b`, 'gi')
    result = result.replace(regex, corrected)
  }

  // Domain-specific smart normalization (avoiding duplicate appended suffixes)
  result = result.replace(/\b(cavaletti\s+rails?|cavaletti)\b/gi, 'Cavaletti rails')
  result = result.replace(/\b(osteoarthritis\s*\(OA\)|osteoarthritis)\b/gi, 'osteoarthritis (OA)')
  result = result.replace(/\b(underwater\s+treadmill\s*\(UWTM\)|underwater\s+treadmill|under\s+water\s+treadmill)\b/gi, 'underwater treadmill (UWTM)')
  result = result.replace(/\b(photobiomodulation\s*\(laser\s+therapy\)|photobiomodulation)\b/gi, 'photobiomodulation (laser therapy)')
  result = result.replace(/\b(cranial\s+cruciate\s+ligament\s*\(CCL\)|cranial\s+cruciate\s+ligament|cranial\s+cruciate)\b/gi, 'cranial cruciate ligament (CCL)')
  result = result.replace(/\b(intervertebral\s+disc\s+disease\s*\(IVDD\)|disc\s+disease)\b/gi, 'intervertebral disc disease (IVDD)')

  return result
}

export interface ClinicalAudioSample {
  id: string
  title: string
  petContext: string
  duration: string
  transcript: string
}

export const CLINICAL_SAMPLE_CONSULTATIONS: ClinicalAudioSample[] = [
  {
    id: 'sample-tplo-week3',
    title: 'Post-Op TPLO Week 3 Follow-Up',
    petContext: 'Buddy (Golden Retriever, 4 yrs)',
    duration: '0:45',
    transcript:
      'Consultation assessment for Buddy, week 3 post-op right TPLO. Owner reports Buddy is bearing 80% weight on the right hind limb at home and completed 90% of prescribed home exercises. Morning stiffness is noticeably reduced down to 3 out of 10, with pain well controlled at 2 out of 10. On physical examination, incision is clean with no joint effusion. Right stifle extension PROM measured at 135 degrees. Right thigh circumference is 38 centimeters compared to 40 centimeters on the contralateral limb. Gait shows Grade 1 lameness at walk. Treatment performed today: 15 minutes of gentle myofascial release on gluteal and quadriceps musculature, photobiomodulation laser therapy to right stifle at 4 J/cm², and 10 minutes on underwater treadmill at 1.2 mph with water at stifle level. For our plan, continue daily home PROM 2 times daily, introduce sit-to-stand squats 10 reps twice daily, and schedule next hydrotherapy session in 10 days.'
  },
  {
    id: 'sample-ivdd-rehab',
    title: 'IVDD Conservative Management & Gait',
    petContext: 'Bella (Dachshund, 6 yrs)',
    duration: '0:38',
    transcript:
      'Session record for Bella, presenting for conservative rehabilitation of Stage 2 thoracolumbar IVDD. Owner observed steady motor improvement; Bella is walking without knuckling and energy level is normal. Pain score is evaluated at 2 out of 10 and stiffness score is 2 out of 10. Objective findings: Proprioceptive placing is intact bilaterally in pelvic limbs with slight delay on left. Mild muscle spasms along T11 to L2 epaxial muscles. Lameness grade is 1 out of 5. Action taken today: Thoracolumbar myofascial soft tissue release, Class IV laser therapy along lumbar spine, and 5 minutes of Airex balance disc standing to encourage core engagement. Plan: Continue strict crate rest between exercises, perform 3 sets of 30 second balance disc standing daily, and recheck in 2 weeks.'
  },
  {
    id: 'sample-senior-osteoarthritis',
    title: 'Senior Canine Osteoarthritis Maintenance',
    petContext: 'Max (Labrador, 11 yrs)',
    duration: '0:42',
    transcript:
      'Re-evaluation for Max, chronic bilateral hip and stifle osteoarthritis. Owner mentions Max was slower rising after cold mornings, but overall mobility improved after starting daily supplements. Stiffness score recorded at 5 out of 10 and pain score at 3 out of 10. Physical exam: Crepitus in both coxofemoral joints and mild restricted carpal extension. Stifle extension ROM measured at 125 degrees. Treatment: Applied thermotherapy over lumbar spine and hips, gentle passive stretching, followed by 12 minutes in the underwater treadmill at low resistance. Treatment plan: Maintain 2x weekly underwater treadmill sessions, recommend non-slip rugs at home, and review pain medication schedule with referring veterinarian in 3 weeks.'
  }
]
