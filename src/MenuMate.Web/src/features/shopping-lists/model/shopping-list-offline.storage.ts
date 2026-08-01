import {
  isOfflineShoppingRecord,
  type OfflineShoppingRecord,
} from "@/features/shopping-lists/model/shopping-list-offline"

const databaseName = "menumate-shopping-offline"
const databaseVersion = 1
const storeName = "shopping"
const currentRecordKey = "current"

let databasePromise: Promise<IDBDatabase> | null = null
let writeQueue: Promise<void> = Promise.resolve()

export async function readOfflineShoppingRecord() {
  await writeQueue

  if (!("indexedDB" in globalThis)) {
    return null
  }

  const database = await openDatabase()
  const transaction = database.transaction(storeName, "readonly")
  const transactionCompleted = transactionToPromise(transaction)
  const request = transaction.objectStore(storeName).get(currentRecordKey) as IDBRequest<unknown>
  const value = await requestToPromise(request)
  await transactionCompleted
  return isOfflineShoppingRecord(value) ? value : null
}

export function writeOfflineShoppingRecord(record: OfflineShoppingRecord) {
  return enqueueWrite(async () => {
    if (!("indexedDB" in globalThis)) {
      return
    }

    const database = await openDatabase()
    const transaction = database.transaction(storeName, "readwrite")
    transaction.objectStore(storeName).put(record, currentRecordKey)
    await transactionToPromise(transaction)
  })
}

export function clearOfflineShoppingRecord() {
  return enqueueWrite(async () => {
    if (!("indexedDB" in globalThis)) {
      return
    }

    const database = await openDatabase()
    const transaction = database.transaction(storeName, "readwrite")
    transaction.objectStore(storeName).delete(currentRecordKey)
    await transactionToPromise(transaction)
  })
}

function enqueueWrite(operation: () => Promise<void>) {
  const result = writeQueue.then(operation)
  writeQueue = result.catch(() => undefined)
  return result
}

function openDatabase() {
  databasePromise ??= new Promise<IDBDatabase>((resolve, reject) => {
    const request = globalThis.indexedDB.open(databaseName, databaseVersion)

    request.onupgradeneeded = () => {
      const database = request.result
      if (!database.objectStoreNames.contains(storeName)) {
        database.createObjectStore(storeName)
      }
    }
    request.onsuccess = () => {
      resolve(request.result)
    }
    request.onerror = () => {
      reject(request.error ?? new Error("Не удалось открыть офлайн-хранилище."))
    }
  })

  return databasePromise
}

function requestToPromise<T>(request: IDBRequest<T>) {
  return new Promise<T>((resolve, reject) => {
    request.onsuccess = () => {
      resolve(request.result)
    }
    request.onerror = () => {
      reject(request.error ?? new Error("Не удалось прочитать офлайн-хранилище."))
    }
  })
}

function transactionToPromise(transaction: IDBTransaction) {
  return new Promise<void>((resolve, reject) => {
    transaction.oncomplete = () => {
      resolve()
    }
    transaction.onerror = () => {
      reject(transaction.error ?? new Error("Операция с офлайн-хранилищем завершилась ошибкой."))
    }
    transaction.onabort = () => {
      reject(transaction.error ?? new Error("Операция с офлайн-хранилищем была отменена."))
    }
  })
}
